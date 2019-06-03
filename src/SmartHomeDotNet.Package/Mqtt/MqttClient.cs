using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mqtt;
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

		private readonly Utils.AsyncLock _connectionGate = new Utils.AsyncLock();
		private Connection _connection;
		private int _connectionClients = 0;

		public MqttClient(
			MqttBrokerConfig broker,
			IScheduler messagesScheduler,
			params string[] autoSubscribeRootTopics)
			: this(broker, null, messagesScheduler, autoSubscribeRootTopics)
		{
		}

		public MqttClient(
			MqttBrokerConfig broker,
			bool? isTestEnvironment,
			IScheduler messagesScheduler,
			params string[] autoSubscribeRootTopics)
		{
			_broker = broker;
			_scheduler = messagesScheduler;
			_rootTopics = autoSubscribeRootTopics;

#if DEBUG
			IsTestEnvironment = isTestEnvironment ?? true;
#else
			IsTestEnvironment = isTestEnvironment ?? Debugger.IsAttached;
#endif
		}

		/// <summary>
		/// Gets the availability topic of this client
		/// </summary>
		public string AvailabilityTopic => _broker.ClientStatusTopic;

		internal bool IsTestEnvironment { get; }
		/*
		/// <summary>
		/// Gets an observable sequence which indicates if the client is enabled or not.
		/// </summary>
		/// <remarks>This will become 'false' if another client goes 'online' on topic "<see cref="AvailabilityTopic"/>_DEBUG"</remarks>
		/// <returns></returns>
		public IObservable<bool> GetAndObserveIsEnabled()
			=> ObserveEvent(AvailabilityTopic + "_DEBUG").Select(value => topic.Value != "online");
		*/
		internal IObservable<bool> GetAndObserveIsEnabled() => Observable.Return(true, Scheduler.Immediate);

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
					await Enable(ct);

					var mqttTopic = await _connection.Subscribe(ct, topic);
					if (mqttTopic.HasValue)
					{
						return mqttTopic
							.ObserveUpdates()
							.StartWith(Scheduler.Immediate, default((MqttTopic topic, string value)))
							.Select(changed => mqttTopic.ToImmutable(changed.topic, changed.value));
					}
					else
					{
						return mqttTopic
							.ObserveUpdates()
							.Select(changed => mqttTopic.ToImmutable(changed.topic, changed.value));
					}
				})
				.Finally(() => _scheduler.ScheduleAsync((_, ct) => Release(ct)));
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
					$"The device id '{topic}' must be fully qualified (i.e. you cannot use wildcards).");
			}

			return Observable
				.DeferAsync(async ct =>
				{
					await Enable(ct);

					var mqttTopic = await _connection.Subscribe(ct, topic, QualityOfService.ExcatlyOnce);

					return mqttTopic
						.ObserveUpdates()
						.Select(update => update.value);
				})
				.Finally(() => _scheduler.ScheduleAsync((_, ct) => Release(ct)));
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
				await Enable(ct);

				await _connection.Publish(ct, topic, value);
			}
			finally
			{
				await Release(ct);
			}
		}

		private async Task Enable(CancellationToken ct)
		{
			using (await _connectionGate.LockAsync(ct))
			{
				if (++_connectionClients == 1)
				{
					ct = CancellationToken.None;
					_connection = await Connection.Create(ct, this);
				}
			}
		}

		private async Task Release(CancellationToken ct)
		{
			using (await _connectionGate.LockAsync(ct))
			{
				if (--_connectionClients == 0)
				{
					var connection = _connection;
					_connection = null;
					await connection.DisposeAsync(ct);
				}
			}
		}

		private class Connection : IDisposable
		{
			private readonly CompositeDisposable _subscriptions = new CompositeDisposable(2);

			private System.Net.Mqtt.IMqttClient _client;
			private MqttCache _topics;
			private IObservable<(bool hasUpdated, MqttTopic updated)> _messages;
			private Task _isReady;
			private IScheduler _scheduler;

			public static async Task<Connection> Create(CancellationToken ct, MqttClient owner)
			{
				var connection = new Connection();
				await connection.AsyncCtor(owner);
				return connection;
			}

			private Connection()
			{
			}

			private async Task AsyncCtor(MqttClient owner)
			{
				var config = new MqttConfiguration
				{
					Port = owner._broker.Port,
					MaximumQualityOfService = MqttQualityOfService.ExactlyOnce,
					KeepAliveSecs = (ushort)(Debugger.IsAttached ? 300 : 10)
				};
				var creds = new MqttClientCredentials(owner._broker.ClientId, owner._broker.Username, owner._broker.Password);
				var will = new MqttLastWill(owner._broker.ClientStatusTopic, MqttQualityOfService.AtLeastOnce, true, Encoding.UTF8.GetBytes("offline"));

				_scheduler = owner._scheduler;
				_topics = new MqttCache();
				_client = await System.Net.Mqtt.MqttClient.CreateAsync(owner._broker.Host, config);

				var messages = CreateMessagesObservable();
				_messages = messages;

				// First subscribe to the message received
				_subscriptions.Add(messages.Connect());

				// Connect to broker
				await _client.ConnectAsync(creds, will, cleanSession: true);

				// Birth message
				var birth = new MqttApplicationMessage(owner._broker.ClientStatusTopic, Encoding.UTF8.GetBytes("online"));
				await _client.PublishAsync(birth, MqttQualityOfService.AtLeastOnce, retain: true);

				// Start discovery
				await Task.WhenAll(owner._rootTopics.Select(t => t.Trim('#', '/') + "/#").Select(topic => _client.SubscribeAsync(topic, MqttQualityOfService.AtLeastOnce)));

				_subscriptions.Add(Disposable.Create(() =>
				{
					if (_client.IsConnected)
					{
						_client.DisconnectAsync().ContinueWith(_ => _client.Dispose());
					}
					else
					{
						_client.Dispose();
					}
				}));

				var wildchars = new[] {'+'};
				if (owner._rootTopics.Any())
				{
					var rootTopicsAreReady = owner
						._rootTopics
						.Select(t => t.Split(wildchars, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim('#', '/'))
						.Distinct()
						.Select(t =>
						{
							var topic = _topics.Get(t);
							var isReady = IsReady(CancellationToken.None, topic);

							return isReady;
						});
					_isReady = Task.WhenAll(rootTopicsAreReady);
				}
				else
				{
					//_isReady = Task.Delay(TimeSpan.FromMilliseconds(100));
					_isReady = Task.CompletedTask;
				}
			}

			private IConnectableObservable<(bool, MqttTopic)> CreateMessagesObservable()
			{
				var closed = Observable
					.FromEventPattern<MqttEndpointDisconnected>(
						h => _client.Disconnected += h,
						h => _client.Disconnected -= h,
						_scheduler)
					.Select(_ => Notification.CreateOnError<(bool, MqttTopic)>(new InvalidOperationException("Connection closed")))
					.Dematerialize();

				var received = _client
					.MessageStream
					.ObserveOn(_scheduler)
					.Select(message => _topics.TryUpdate(
						message.Topic,
						Encoding.UTF8.GetString(message.Payload),
						/* TODO XAMARIN: message.Retain */ true));

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
				// retained messages at startup (at least with Mosquitto with M2MQTT).
				// So we specifically subscribe to the observed topic.
				this.Log().Info("Subscribing to topic: " + topic);

				await _client.SubscribeAsync(topic + "/#", (MqttQualityOfService) qos);

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

				// TODO
				// await _client.PublishAsync(new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(value)), (MqttQualityOfService) qos, retain);
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

			public async Task DisposeAsync(CancellationToken ct)
			{
				await _client.DisconnectAsync();
				Dispose();
			}

			/// <inheritdoc />
			public void Dispose() => _subscriptions.Dispose();
		}
	}
}