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
using Mavri.Utils;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;
using AsyncLock = Mavri.Utils.AsyncLock;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> which uses the MQTT protocol to get updates
	/// </summary>
	public sealed class MqttClient : IDisposable
	{
		// If the connection failed (network issue?) we auto schedule a retry in 1 mn
		// Note: If someone tries to send a message, it will bypass this delay.
		private static readonly TimeSpan _connectionInfiniteRetryDelay = TimeSpan.FromMinutes(1);

		// Publish something on a regular basis to maintain the connection alive
		// Workaround an issue with system.net.mqtt which seems to be disconnected without any notification
		private static readonly TimeSpan _connectionActivePullingDelay = TimeSpan.FromMinutes(1);

		/// <summary>
		/// The delay to wait after the last subscriber on the client disconnected before aborting underlying MQTT broker subscription
		/// </summary>
		private static readonly TimeSpan _connectionAbortDelay = TimeSpan.FromSeconds(5);

		// When sending a message on the current connection, number of tentative before throwing an exception on caller
		private const int _sendMessageTries = 3;

		private readonly MqttBrokerConfig _broker;
		private readonly IScheduler _scheduler;
		private readonly string[] _rootTopics;

		private readonly object _connectionGate = new object();
		private int _connectionClients;
		private Connection _connection;

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

		private Subscription Enable()
		{
			Interlocked.Increment(ref _connectionClients);

			var connection = _connection;
			if (connection == null)
			{
				lock (_connectionGate)
				{
					if (_connection == null)
					{
						_connection = new Connection(_broker, _rootTopics, _scheduler);
					}

					connection = _connection;
				}
			}

			return new Subscription(this, connection);
		}

		private void Release(Connection connection)
		{
			if (_connection != connection)
			{
				throw new InvalidOperationException("Invalid state");
			}

			if (_connectionClients < 3)
			{
				// If there is only few active connections, don't even try to release subscription,
				// instead delay it by 5 sec to avoid fast connect/disconnect due to subscribe while 'Publish'
				_scheduler.Schedule(connection, _connectionAbortDelay, DelayedRelease);
			}
			else if (Interlocked.Decrement(ref _connectionClients) == 0)
			{
				// Unfortunately we released the last connection, pseudo-create a new one
				// then start the delay for the same reasons.

				Interlocked.Increment(ref _connectionClients);
				_scheduler.Schedule(connection, _connectionAbortDelay, DelayedRelease);
			}
		}

		private IDisposable DelayedRelease(IScheduler scheduler, Connection connection)
		{
			if (_connection != connection)
			{
				throw new InvalidOperationException("Invalid state");
			}

			if (Interlocked.Decrement(ref _connectionClients) == 0)
			{
				lock (_connectionGate)
				{
					if (_connectionClients == 0)
					{
						_connection = null;
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
			_connection.Dispose();
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
			private readonly SerialDisposable _restore = new SerialDisposable();
			private readonly object _enablingGate = new object();
			private readonly AsyncLock _clientGate = new AsyncLock(); // Unfortunately, sending multiple message concurrently causes some issue with System.Net.Mqtt

			private readonly MqttBrokerConfig _config;
			private readonly ReplaySubject<MqttConnectionStatus> _status;

			private TaskCompletionSource<IMqttClient> _currentClient;

			/// <inheritdoc />
			public IScheduler Scheduler { get; }

			/// <summary>
			/// The root of the MQTT cache 
			/// </summary>
			public MqttCache Topics { get; }

			public Connection(MqttBrokerConfig config, string[] rootTopics, IScheduler scheduler)
			{
				_config = config;
				Scheduler = scheduler;

				_status = new ReplaySubject<MqttConnectionStatus>(1, System.Reactive.Concurrency.Scheduler.Immediate);
				_status.OnNext(MqttConnectionStatus.Disabled);
				Topics = new MqttCache(this);
				foreach (var rootTopic in rootTopics)
				{
					Topics.Get(rootTopic).AddPermanentRootSubscription();
				}

				Enable(isInitial: true);
			}

			private void OnError(Exception error)
			{
				_status.OnNext(MqttConnectionStatus.Disabled);

				this.Log().Error("MQTT client subscription failed, restore it", error);

				_restore.Disposable = Scheduler.Schedule(() => Enable(isInitial: false));
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
							_subscription.Disposable = Scheduler.ScheduleAsync(isInitial, Enable);
						}
					}
				}
			}

			private async Task<IDisposable> Enable(IScheduler scheduler, bool isInitial, CancellationToken ct)
			{
				this.Log().Info("Enabling MQTT client");

				var subscriptions = new CompositeDisposable(4);
				try
				{
					_status.OnNext(MqttConnectionStatus.Connecting);

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
						.DistinctUntilChanged(MessageComparer.Instance) // WEIRD ! (probably dues to multiple connection from topic and child topics)
						.ObserveOn(scheduler)
						.Do(message => Topics.TryUpdate(message.Topic, message.Payload == null ? null : Encoding.UTF8.GetString(message.Payload), message.Retain))
						.Subscribe(_ => { }, OnError)
						.DisposeWith(subscriptions);

					// Connect to broker
					await client.ConnectAsync(creds, will, cleanSession: isInitial);
					DisconnectAndDispose(client).DisposeWith(subscriptions);

					// Birth message
					await client.PublishAsync(birth, MqttQualityOfService.AtLeastOnce, retain: true);

					// Publish something on a regular basis to maintain the connection alive
					// Workaround an issue with system.net.mqtt which seems to be disconnected without any notification
					Observable
						.Interval(_connectionActivePullingDelay, scheduler)
						.Execute(
							(ct2, _) => Publish(ct, _config.ClientLastSeenTopic, DateTimeOffset.Now.ToString("R"), QualityOfService.AtLeastOnce, retain: false), 
							ConcurrentExecutionMode.AbortPrevious, 
							scheduler)
						.Subscribe(_ => { }, OnError)
						.DisposeWith(subscriptions);

					// We set the client as current, then we request to the cache to 'Probe' in order to
					// initialize/restore all topics subscriptions
					// Note: The cache is still in initializing mode, so it won't publish any update
					_currentClient.TrySetResult(client);
					_status.OnNext(MqttConnectionStatus.Connected);

					return subscriptions;
				}
				catch (Exception e)
				{
					this.Log().Error($"Failed to connect to MQTT broker, will retry in {_connectionInfiniteRetryDelay}.", e);

					_status.OnNext(MqttConnectionStatus.Disabled);
					_currentClient.TrySetException(e);
					subscriptions.Dispose();

					// If the connection failed (network issue?) we auto schedule a retry in 1 mn
					// Note: If someone tries to send a message, it will bypass this delay.
					return scheduler.Schedule(_connectionInfiniteRetryDelay, () => OnError(null));
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

			/// <inheritdoc />
			public IObservable<MqttConnectionStatus> GetAndObserveStatus()
				=> _status.DistinctUntilChanged();

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

				using (await _clientGate.LockAsync(ct))
				{
					await client.SubscribeAsync(topic, MqttQualityOfService.AtLeastOnce);
				}
			}

			public Task Publish(CancellationToken ct, string topic, string value, QualityOfService qos, bool retain)
			{
				this.Log().Debug($"Publishing ({(retain?"retained": "volatile")}) message to topic '{topic}': {value}");

				var message = new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(value), retain);
				Task Send(IMqttClient client) => client.PublishAsync(message, (MqttQualityOfService)qos, retain);

				return UsingCurrentClient(ct, _sendMessageTries, Send, "publishing message on topic " + topic);
			}

			private async Task UsingCurrentClient(
				CancellationToken ct, 
				int tries, 
				Func<IMqttClient, Task> action, 
				string message,
				[CallerMemberName] string caller = null, 
				[CallerLineNumber] int line = -1)
			{
				tries = Math.Max(1, tries);
				int attempt = 0;
				do
				{
					attempt++;

					var clientAsync = _currentClient;
					try
					{
						var client = await clientAsync.Task;
						if (!client.IsConnected)
						{
							throw new Exception("The resolved client is not connected");
						}

						using (await _clientGate.LockAsync(ct))
						{
							await action(client);
						}

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

						// As 'OnError' restores the connection asynchronously, make sure to wait
						// for a new client before retry (with Max yield = 100)
						for (var i = 0; _currentClient == clientAsync && !ct.IsCancellationRequested && i < 100; i++)
						{
							await Scheduler.Yield(ct);
						}
					}
				} while (!ct.IsCancellationRequested);
			}

			/// <inheritdoc />
			public void Dispose()
			{
				this.Log().Info("Disposing MQTT subscription");

				_restore.Dispose();
				_subscription.Dispose();

				_status.OnNext(MqttConnectionStatus.Disabled);
				_status.OnCompleted();
				_status.Dispose();
			}
		}
	}
}