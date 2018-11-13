using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> which uses the MQTT protocol to get updates
	/// </summary>
	public sealed class MqttClient
	{
		private readonly MqttBrokerConfig _broker;
		private readonly IScheduler _scheduler;
		private readonly string[] _rootTopics;

		private readonly object _connectionGate = new object();
		private Connection _connection;
		private int _connectionClients = 0;

		public MqttClient(
			MqttBrokerConfig broker,
			IScheduler messagesScheduler,
			params string[] autoSubscribeRootTopics)
		{
			_broker = broker;
			_scheduler = messagesScheduler;
			_rootTopics = autoSubscribeRootTopics;
		}

		/// <summary>
		/// Gets the availability topic of this client
		/// </summary>
		public string AvailabilityTopic => _broker.ClientStatusTopic;

		/// <summary>
		/// Gets an observable sequence of the state of a topic
		/// </summary>
		/// <param name="topic">The topic name to subscribe</param>
		/// <returns>An observable sequence that produces a value each time the value or any sub topic changes probed with the initial current state.</returns>
		public IObservable<MqttTopicValues> GetAndObserveState(string topic)
		{
			if (topic.Contains('#') || topic.Contains('+'))
			{
				throw new ArgumentOutOfRangeException(
					nameof(topic),
					$"The device id '{topic}' must be fully qualified (i.e. you cannot use wilcards).");
			}

			return Observable
				.DeferAsync(async ct =>
				{
					Enable();

					var mqttTopic = await _connection.Subscribe(ct, topic);

					return mqttTopic
						.ObserveUpdates()
						.StartWith(Scheduler.Immediate, default((MqttTopic, string)))
						.Select(_ => mqttTopic.ToImmutable());
				})
				.Finally(Release);
		}

		/// <summary>
		/// Gets an observable sequence of the value of a topic
		/// </summary>
		/// <param name="topic">The topic name to subscribe</param>
		/// <returns>An observable sequence that produces a value each time the value changes</returns>
		public IObservable<string> ObserveEvent(string topic)
		{
			if (topic.Contains('#') || topic.Contains('+'))
			{
				throw new ArgumentOutOfRangeException(
					nameof(topic),
					$"The device id '{topic}' must be fully qualified (i.e. you cannot use wilcards).");
			}

			return Observable
				.DeferAsync(async ct =>
				{
					Enable();

					var mqttTopic = await _connection.Subscribe(ct, topic, QualityOfService.ExcatlyOnce);

					return mqttTopic
						.ObserveUpdates()
						.Select(update => update.value);
				})
				.Finally(Release);
		}

		/// <summary>
		/// Publishes a value to the MQTT broker
		/// </summary>
		/// <param name="ct">A token to abort the asynchronous operations</param>
		/// <param name="topic">The topic to which the messages has to be published</param>
		/// <param name="value">The value of the message</param>
		/// <param name="qos">The quality of service requested for this message</param>
		/// <param name="retain">Defines if the message should be retained or not</param>
		/// <returns>An asynchronous operation</returns>
		public async Task Publish(CancellationToken ct, string topic, string value, QualityOfService qos = QualityOfService.AtLeastOnce, bool retain = true)
		{
			try
			{
				Enable();

				await _connection.Publish(ct, topic, value);
			}
			finally
			{
				Release();
			}
		}

		private void Enable()
		{
			lock (_connectionGate)
			{
				if (++_connectionClients == 1)
				{
					_connection = new Connection(this);
				}
			}
		}

		private void Release()
		{
			lock (_connectionGate)
			{
				if (--_connectionClients == 0)
				{
					var connection = _connection;
					_connection = null;
					connection.Dispose();
				}
			}
		}

		private class Connection : IDisposable
		{
			private readonly CompositeDisposable _subscriptions = new CompositeDisposable(2);

			private readonly uPLibrary.Networking.M2Mqtt.MqttClient _client;
			private readonly MqttCache _topics;
			private readonly IObservable<(bool hasUpdated, MqttTopic updated)> _messages;
			private readonly Task _isReady;
			private readonly IScheduler _scheduler;

			public Connection(MqttClient owner)
			{
				_scheduler = owner._scheduler;
				_topics = new MqttCache();
				_client = new uPLibrary.Networking.M2Mqtt.MqttClient(owner._broker.Host, owner._broker.Port, false, null, null, MqttSslProtocols.None);

				var messages = CreateMessagesObservable();
				_messages = messages;

				// First subscribe to the message received
				_subscriptions.Add(messages.Connect());

				// Connect to broker
				_client.Connect(
					owner._broker.ClientId,
					owner._broker.Username,
					owner._broker.Password,
					willRetain: true,
					willQosLevel: (byte)QualityOfService.AtLeastOnce,
					willFlag: true,
					willTopic: owner._broker.ClientStatusTopic,
					willMessage: "offline",
					cleanSession: true,
					keepAlivePeriod: (ushort)(Debugger.IsAttached ? 300 : 10));

				// Birth messsage
				_client.Publish(
					owner._broker.ClientStatusTopic,
					Encoding.UTF8.GetBytes("online"),
					(byte)QualityOfService.AtLeastOnce,
					retain: true);

				// Start discovery
				_client.Subscribe(
					owner._rootTopics.Select(t => t.Trim('#', '/') + "/#").ToArray(),
					Enumerable.Repeat((byte)QualityOfService.AtLeastOnce, owner._rootTopics.Length).ToArray());

				_subscriptions.Add(Disposable.Create(() =>
				{
					if (_client.IsConnected)
					{
						_client.Disconnect();
					}
				}));

				var wildchars = new[] {'+'};
				var rootTopicsAreReady = owner
					._rootTopics
					.Select(t =>
					{
						var name = t.Split(wildchars, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim('#', '/');
						var topic = _topics.Get(name);
						var isReady = IsReady(CancellationToken.None, topic);

						return isReady;
					});
				_isReady = Task.WhenAll(rootTopicsAreReady);
			}

			private IConnectableObservable<(bool, MqttTopic)> CreateMessagesObservable()
			{
				var closed = Observable
					.FromEventPattern<uPLibrary.Networking.M2Mqtt.MqttClient.ConnectionClosedEventHandler, EventArgs>(
						h => _client.ConnectionClosed += h,
						h => _client.ConnectionClosed -= h,
						_scheduler)
					.Select(_ => Notification.CreateOnError<(bool, MqttTopic)>(new InvalidOperationException("Connection closed")))
					.Dematerialize();

				var received = Observable
					.FromEventPattern<uPLibrary.Networking.M2Mqtt.MqttClient.MqttMsgPublishEventHandler, MqttMsgPublishEventArgs>(
						h => _client.MqttMsgPublishReceived += h,
						h => _client.MqttMsgPublishReceived -= h,
						_scheduler)
					.ObserveOn(_scheduler)
					.Select(message => _topics.TryUpdate(
						message.EventArgs.Topic, 
						Encoding.UTF8.GetString(message.EventArgs.Message), 
						message.EventArgs.Retain));

				return received.Merge(closed).Publish();
			}

			public IObservable<(bool hasUpdated, MqttTopic updated)> ObserveMessages() => _messages;

			public async Task<MqttTopic> Subscribe(CancellationToken ct, string topic, QualityOfService qos = QualityOfService.AtLeastOnce)
			{
				// First make sure to wait for all the initial messages to be processed
				if (!_isReady.IsCompleted)
				{
					await _isReady;
				}

				// Even if we are already subscribed to "rootTopic/#", it's common to not receive all 
				// retained messages at startup (at least avec Mosquitto with M2MQTT).
				// So we sepecifically subscribe to the oberved topic.
				this.Log().Info("Subscribing to topic: " + topic);

				_client.Subscribe(
					new[] { topic + "/#" },
					new[] { (byte)qos });

				var mqttTopic = _topics.Get(topic);

				// Again, make sure to have received all the retained messages for the requested topic
				await IsReady(ct, mqttTopic);

				return mqttTopic;
			}

			public async Task Publish(CancellationToken ct, string topic, string value, QualityOfService qos = QualityOfService.AtLeastOnce, bool retain = true)
			{
				// First make sure to wait for all the initial messages to be processed
				if (!_isReady.IsCompleted)
				{
					await _isReady;
				}

				this.Log().Info($"Publishing '{value}' to topic '{topic}'.");

				_client.Publish(topic, Encoding.UTF8.GetBytes(value), (byte) qos, retain);
				_topics.TryUpdate(topic, value, retain);
			}

			private async Task IsReady(CancellationToken ct, MqttTopic topic)
			{
				var notUpdatedSinceAFew = topic
					.ObserveUpdates()
					.Throttle(TimeSpan.FromMilliseconds(100), _scheduler);
				var timeout = Observable
					.Timer(topic.HasValue ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(5), _scheduler)
					.Select(_ => default((MqttTopic, string)));

				await notUpdatedSinceAFew.Amb(timeout).FirstAsync().ToTask(ct);
			}

			/// <inheritdoc />
			public void Dispose() => _subscriptions.Dispose();
		}
	}
}