using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mqtt;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;
using AsyncLock = SmartHomeDotNet.Utils.AsyncLock;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> which uses the MQTT protocol to get updates
	/// </summary>
	public sealed class MqttClient : IDisposable
	{
		private readonly MqttBrokerConfig _broker;
		private readonly IScheduler _scheduler;
		private readonly string[] _rootTopics;

		private readonly Utils.AsyncLock _connectionGate = new Utils.AsyncLock();
		//private Connection _connection;
		private int _connectionClients = 0;
		private Connection _subscription;

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
			_rootTopics = autoSubscribeRootTopics.Select(ToSubscribeTopic).ToArray();

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
		public IObservable<MqttTopicValues> GetAndObserveTopic(string topic)
		{
			return Observable.Using(Enable, mqtt => mqtt.Get(topic).GetAndObserve());
		}

		/// <summary>
		/// Gets an observable sequence of the value of a topic
		/// </summary>
		/// <param name="topic">The topic name to subscribe</param>
		/// <returns>An observable sequence that produces a value each time the value changes</returns>
		public IObservable<string> ObserveTopic(string topic)
		{
			topic = ToSubscribeTopic(topic);

			return Observable.Using(Enable, mqtt => mqtt.Get(topic).ObserveLocalValue());
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
			using (var mqtt = Enable())
			{
				await mqtt.Publish(ct, topic, value, qos, retain);
			}
		}

		private object _connection2Gate = new object();
		private int _connections;
		private Subscription Enable()
		{
			Interlocked.Increment(ref _connectionClients);

			var connection = _subscription;
			if (connection == null)
			{
				lock (_connection2Gate)
				{
					if (_subscription == null)
					{
						_subscription = new Connection(_broker, _rootTopics, _scheduler);
					}

					connection = _subscription;
				}
			}

			return new Subscription(this, connection);
		}

		private void Release(Connection connection)
		{
			if (_subscription != connection)
			{
				throw new InvalidOperationException("Invalid state");
			}

			if (_connectionClients < 3)
			{
				// If there is only few active connections, don't even try to release subscription,
				// instead delay it by 5 sec to avoid fast connect/disconnect due to subscribe while 'Publish'
				_scheduler.Schedule(connection, TimeSpan.FromSeconds(5), DelayedRelease);
			}
			else if (Interlocked.Decrement(ref _connectionClients) == 0)
			{
				// Unfortunately we released the last connection, pseudo-create a new one
				// then start the delay for the same reasons.

				Interlocked.Increment(ref _connectionClients);
				_scheduler.Schedule(connection, TimeSpan.FromSeconds(5), DelayedRelease);
			}
		}

		private IDisposable DelayedRelease(IScheduler scheduler, Connection connection)
		{
			if (_subscription != connection)
			{
				throw new InvalidOperationException("Invalid state");
			}

			if (Interlocked.Decrement(ref _connectionClients) == 0)
			{
				lock (_connection2Gate)
				{
					if (_connectionClients == 0)
					{
						_subscription = null;
						connection.Dispose();
					}
				}
			}

			return Disposable.Empty;
		}

		private static string ToSubscribeTopic(string topic)
		{
			topic = topic.TrimEnd('#', '/');

			if (topic.Contains('#') || topic.Contains('+'))
			{
				throw new ArgumentOutOfRangeException(nameof(topic), $"The topic '{topic}' must be fully qualified (i.e. you cannot use wildcards).");
			}

			return topic;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription.Dispose();
		}


		private class Subscription : IDisposable
		{
			private readonly MqttClient _owner;
			private readonly Connection _connection;

			public Subscription(MqttClient owner, Connection connection)
			{
				_owner = owner;
				_connection = connection;
			}

			public MqttCacheTopic Get(string topic) 
				=> _connection.Topics.Get(topic);

			public Task Publish(CancellationToken ct, string topic, string value, QualityOfService qos, bool retain)
				=> _connection.Publish(ct, topic, value, qos, retain);

			/// <inheritdoc />
			public void Dispose() => _owner.Release(_connection);
		}

		private class Connection : IDisposable, IMqttConnection
		{
			private readonly SerialDisposable _subscription = new SerialDisposable();
			private readonly MqttBrokerConfig _config;
			private readonly object _enablingGate = new object();

			private TaskCompletionSource<IMqttClient> _currentClient;

			/// <inheritdoc />
			public IScheduler Scheduler { get; }

			public MqttCache Topics { get; }

			public Connection(MqttBrokerConfig config, string[] rootTopics, IScheduler scheduler)
			{
				_config = config;
				Scheduler = scheduler;

				Topics = new MqttCache(this);
				foreach (var rootTopic in rootTopics)
				{
					Topics.Get(rootTopic).AddPermanentRootSubscription();
				}

				Enable(isInitial: true);
			}

			private void Enable(bool isInitial)
			{
				if (_currentClient == null || _currentClient.Task.IsCompleted || _currentClient.Task.IsFaulted)
				{
					lock (_enablingGate)
					{
						if (_currentClient == null || _currentClient.Task.IsCompleted || _currentClient.Task.IsFaulted)
						{
							_currentClient = new TaskCompletionSource<IMqttClient>();
							_subscription.Disposable = Disposable.Empty; // Make sure to never have 2 subscriptions alive at the same time
							_subscription.Disposable = Scheduler.ScheduleAsync(isInitial, RunAsync);
						}
					}
				}
			}

			private void OnError(Exception error)
			{
				this.Log().Error("MQTT client subscription failed, restore it", error);

				Enable(isInitial: false);
			}

			private async Task<IDisposable> RunAsync(IScheduler scheduler, bool isInitial, CancellationToken ct)
			{
				this.Log().Info("Enabling MQTT client");

				var subscriptions = new CompositeDisposable(3);
				try
				{ 
					using (Topics.Initialize())
					{
						var config = new MqttConfiguration
						{
							Port = _config.Port,
							MaximumQualityOfService = MqttQualityOfService.ExactlyOnce,
							KeepAliveSecs = (ushort) (Debugger.IsAttached ? 300 : 10)
						};
						var creds = new MqttClientCredentials(_config.ClientId, _config.Username, _config.Password);
						var birth = new MqttApplicationMessage(_config.ClientStatusTopic, Encoding.UTF8.GetBytes("online"));
						var will = new MqttLastWill(_config.ClientStatusTopic, MqttQualityOfService.AtLeastOnce, true, Encoding.UTF8.GetBytes("offline"));
						var client = await System.Net.Mqtt.MqttClient.CreateAsync(_config.Host, config);

						// First subscribe to the disconnection
						Observable
							.FromEventPattern<MqttEndpointDisconnected>(
								h => client.Disconnected += h,
								h => client.Disconnected -= h,
								scheduler)
							.Do(_ => OnError(new InvalidOperationException("Connection closed")))
							.Subscribe(_ => { }, OnError)
							.DisposeWith(subscriptions);

						// Then subscribe to the message received
						client
							.MessageStream
							.DistinctUntilChanged(MessageComparer.Instance) // WEIRD !
							.ObserveOn(scheduler)
							.Do(message => Topics.TryUpdate(message.Topic, message.Payload == null ? null : Encoding.UTF8.GetString(message.Payload), message.Retain))
							.Subscribe(_ => { }, OnError)
							.DisposeWith(subscriptions);

						// Connect to broker
						await client.ConnectAsync(creds, will, cleanSession: isInitial);
						DisconnectAndDispose(client).DisposeWith(subscriptions);

						// Birth message
						await client.PublishAsync(birth, MqttQualityOfService.AtLeastOnce, retain: true);

						// We set the client as current, then we request to the cache to 'Probe' in order to
						// initialize/restore all topics subscriptions
						// Note: The cache is still in initializing mode, so it won't publish any update
						_currentClient.TrySetResult(client);

						return subscriptions;
					}
				}
				catch (Exception e)
				{
					this.Log().Error("Failed to connect to MQTT broker", e);

					_currentClient.TrySetException(e);
					subscriptions.Dispose();

					return Disposable.Empty;
				}

				IDisposable DisconnectAndDispose(IMqttClient client) => Disposable.Create(() =>
				{
					if (client.IsConnected)
					{
						client.DisconnectAsync().ContinueWith(disconnection =>
						{
							if (disconnection.IsFaulted)
							{
								this.Log().Error("Failed to disconnect client", disconnection.Exception);
							}

							client.DisposeOrLog("Failed to dispose the client");
						});
					}
					else
					{
						client.DisposeOrLog("Failed to dispose the client");
					}
				});
			}

			private class MessageComparer : IEqualityComparer<MqttApplicationMessage>
			{
				public static MessageComparer Instance { get; } = new MessageComparer();

				/// <inheritdoc />
				public bool Equals(MqttApplicationMessage x, MqttApplicationMessage y)
					=> x == null
						? y == null
						: (y != null
							&& x.Topic == y.Topic
							&& x.Retain == y.Retain
							&& (x.Payload?.SequenceEqual(y.Payload) ?? y.Payload == null));

				/// <inheritdoc />
				public int GetHashCode(MqttApplicationMessage obj)
					=> obj?.Topic.GetHashCode() ?? 0;
			}

			private AsyncLock _gate = new AsyncLock();

			public async Task Subscribe(CancellationToken ct, string topic)
			{
				if (!_currentClient.Task.IsCompleted)
				{
					throw new InvalidOperationException("Cannot subscribe to a topic a this point");
				}

				var client = _currentClient.Task.Result;
				if (!client.IsConnected)
				{
					throw new Exception("The resolved client is not connected");
				}

				topic = topic + "/#";
				this.Log().Info("Subscribing to topic: " + topic);

				using (await _gate.LockAsync(ct))
				{
					await client.SubscribeAsync(topic, MqttQualityOfService.AtLeastOnce);
				}
			}

			public Task Publish(CancellationToken ct, string topic, string value, QualityOfService qos, bool retain)
			{
				this.Log().Info($"Publishing ({(retain?"retained": "volatile")}) message to topic '{topic}': {value}");

				var message = new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(value), retain);

				return Retry(ct, 3, Send, "publishing message on topic " + topic);

				async Task Send()
				{
					var client = await _currentClient.Task;
					if (!client.IsConnected)
					{
						throw new Exception("The resolved client is not connected");
					}

					using (await _gate.LockAsync(ct))
					{
						await client.PublishAsync(message, (MqttQualityOfService)qos, retain);
					}
				}
			}

			private async Task Retry(
				CancellationToken ct, 
				int tries, 
				Func<Task> method, 
				string message,
				[CallerMemberName] string caller = null, 
				[CallerLineNumber] int line = -1)
			{
				tries = Math.Max(1, tries);
				int attempt = 0;
				do
				{
					attempt++;

					try
					{
						await method();

						return;
					}
					catch (Exception e)
					{
						this.Log().Error($"Error while {message} ({caller}@{line}, attempt {attempt} of {tries})", e);

						OnError(e);

						if (attempt == tries)
						{
							throw;
						}
					}
				} while (true);
			}

			/// <inheritdoc />
			public void Dispose() => _subscription.Dispose();
		}
	}
}