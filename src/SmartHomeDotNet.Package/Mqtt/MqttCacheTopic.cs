using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Mqtt
{
	internal sealed class MqttCacheTopic : IDisposable
	{
		private enum MqttTopicStatus
		{
			Disabled,

			Buffering,

			Live
		}

		private readonly object _gate = new object();
		private readonly Subject<string> _localUpdates = new Subject<string>();
		private readonly Subject<MqttTopicValues> _fullUpdates = new Subject<MqttTopicValues>();
		private readonly Subject<Unit> _msgReceived = new Subject<Unit>();
		private readonly Subject<bool> _hasSubscribers = new Subject<bool>();

		private readonly IMqttConnection _connection;
		private readonly IObservable<MqttTopicStatus> _status;
		private readonly IDisposable _subscription;

		private int _subscriptions;
		private ImmutableDictionary<string, MqttCacheTopic> _children = ImmutableDictionary<string, MqttCacheTopic>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
		private bool _hasLocalValue;
		private string _localValue;

		/// <summary>
		/// The full topic path
		/// </summary>
		public string Topic { get; }

		/// <summary>
		/// The level of this topic (i.e. the last part of the <see cref="Topic"/>)
		/// </summary>
		public string Level { get; }

		/// <summary>
		/// The parent topic, or 'null' if this topic is root
		/// </summary>
		public MqttCacheTopic Parent { get; }

		/// <summary>
		/// Indicates if this topic or any of its children has a retained value
		/// </summary>
		public bool HasValue => _hasLocalValue || _children.Any(c => c.Value.HasValue);

		/// <summary>
		/// Creates a root topic
		/// </summary>
		public MqttCacheTopic(IMqttConnection connection)
		{
			_connection = connection;
		}

		private MqttCacheTopic(IMqttConnection connection, MqttCacheTopic parent, string level)
		{
			_connection = connection;

			Level = level;
			Parent = parent;
			Topic = parent.Topic == null //i.e. Root
				? level
				: parent.Topic + "/" + level;

			var engine = Run().Replay(1, Scheduler.Immediate);
			_subscription = engine.Connect();
			_status = engine;
		}

		/// <summary>
		/// Mark this topic as permanently subscribed, so it will be auto re-subscribed each time the connection is restored.
		/// </summary>
		/// <remarks>This won't create subscription when invoked, you have to run Hold/Flush updates sequence to enable subscription</remarks>
		public void AddPermanentRootSubscription() 
			=> Interlocked.Increment(ref _subscriptions);

		/// <summary>
		/// Gets a create a child topic
		/// </summary>
		/// <param name="level">The name of the sub topic</param>
		/// <returns></returns>
		public MqttCacheTopic GetChild(string level)
			=> ImmutableInterlocked.GetOrAdd(ref _children, level, l => new MqttCacheTopic(_connection, this, l));

		#region Subscription
		private IObservable<MqttTopicStatus> GetAndObserveStatus() => _status;

		private IObservable<MqttTopicStatus> Run()
		{
			return Observable
				.CombineLatest(
					_connection.GetAndObserveStatus().DistinctUntilChanged(),
					_hasSubscribers.DistinctUntilChanged(),
					GetStatus)
				.Switch()
				.Retry(TimeSpan.FromSeconds(5), _connection.Scheduler)
				.DistinctUntilChanged();

			IObservable<MqttTopicStatus> GetStatus(MqttConnectionStatus status, bool hasSubscribers)
			{
				switch (status)
				{
					case MqttConnectionStatus.Connected when hasSubscribers:
						return Connect();

					default:
						return Observable.Return(MqttTopicStatus.Disabled, Scheduler.Immediate);
				}
			}

			IObservable<MqttTopicStatus> Connect()
				=> Observable.Create<MqttTopicStatus>(async (observer, ct) =>
				{
					observer.OnNext(MqttTopicStatus.Buffering);

					// Connect
					try
					{
						await _connection.Subscribe(ct, Topic);
					}
					catch (Exception e)
					{
						this.Log().Error(
							$"Failed to subscribe to the topic {Topic}, however the topic remains available "
							+ "(as it may be part of larger subscription). The connection may also have restarted by itself.",
							e);
					}

					// Buffer "retained" changes
					try
					{
						var notUpdatedSinceAFew = this
							._msgReceived
							.Throttle(TimeSpan.FromMilliseconds(100), _connection.Scheduler);
						var timeout = Observable
							.Timer(HasValue ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(5), _connection.Scheduler)
							.Select(_ => Unit.Default);

						await notUpdatedSinceAFew.Amb(timeout).FirstAsync().ToTask(ct);
					}
					catch (Exception e)
					{
						this.Log().Error($"Buffering of topic '{Topic}' failed, however the topic remains available.", e);
					}

					observer.OnNext(MqttTopicStatus.Live);
				});
		}

		private IDisposable Subscribe()
		{
			if (Interlocked.Increment(ref _subscriptions) == 1)
			{
				lock (_hasSubscribers)
				{
					_hasSubscribers.OnNext(true);
				}
			}

			return Disposable.Create(Unsubscribe);

			void Unsubscribe()
			{
				if (Interlocked.Decrement(ref _subscriptions) == 0)
				{
					lock (_hasSubscribers)
					{
						if (_subscriptions == 0)
						{
							_hasSubscribers.OnNext(false);
						}
					}
				}
			}
		}
		#endregion

		#region [GetAnd]Observe topic values
		public IObservable<MqttTopicValues> GetAndObserve() => Observe(prependInitial: true);
		public IObservable<MqttTopicValues> Observe() => Observe(prependInitial: false);

		public IObservable<MqttTopicValues> Observe(bool prependInitial)
			=> Observable.Create<MqttTopicValues>(observer =>
			{
				var subscriptions = new CompositeDisposable();

				lock (_gate)
				{
					Observable
						.CombineLatest(
							_fullUpdates,
							GetAndObserveStatus(),
							(value, status) => (value, status))
						.Where(x => x.status == MqttTopicStatus.Live)
						.Select(x => x.value)
						.Subscribe(observer)
						.DisposeWith(subscriptions);

					if (prependInitial && HasValue)
					{
						observer.OnNext(ToImmutable());
					}

					Subscribe().DisposeWith(subscriptions);

					return subscriptions;
				}
			});

		public IObservable<string> GetAndObserveLocalValue() => ObserveLocalValue(prependInitial: true);
		public IObservable<string> ObserveLocalValue() => ObserveLocalValue(prependInitial: false);

		public IObservable<string> ObserveLocalValue(bool prependInitial)
			=> Observable.Create<string>(observer =>
			{
				var subscriptions = new CompositeDisposable();

				lock (_gate)
				{
					Observable
						.CombineLatest(
							_localUpdates,
							GetAndObserveStatus(),
							(value, status) => (value, status))
						.Where(x => x.status == MqttTopicStatus.Live)
						.Select(x => x.value)
						.Subscribe(observer)
						.DisposeWith(subscriptions);

					if (prependInitial && _hasLocalValue)
					{
						observer.OnNext(_localValue);
					}

					Subscribe().DisposeWith(subscriptions);

					return subscriptions;
				}
			});
		#endregion

		#region Update
		public bool TryUpdate(string value, bool isRetained)
		{
			lock (_gate)
			{
				// If we already had a retained value (stored as local), we are still persist it, no matter the 'retained' flag
				// However, I'm not sure if this is really required nor even a good idea ...
				// Note: This is needed as HA seems to not publish updates as retained (missing config?)
				isRetained |= _hasLocalValue;

				if (!isRetained)
				{
					OnUpdated();

					return true;
				}
				else if (_hasLocalValue && (_localValue?.Equals(value, StringComparison.InvariantCultureIgnoreCase) ?? value == null))
				{
					return false;
				}
				else
				{
					_localValue = value;
					_hasLocalValue = true; // set it after, so we don't have use a lock on state when checking the value

					OnUpdated();

					return true;
				}
			}

			void OnUpdated()
			{
				// _gate is already acquired

				_msgReceived.OnNext(Unit.Default);

				if (_subscriptions > 0) // Do not 'ToImmutable' if no subscribers
				{
					_localUpdates.OnNext(value);
					_fullUpdates.OnNext(ToImmutable(this, value, isRetained));
				}

				Parent?.OnChildUpdated(this, value, isRetained);
			}
		}

		private void OnChildUpdated(MqttCacheTopic topic, string value, bool isRetained)
		{
			lock (_gate)
			{
				_msgReceived.OnNext(Unit.Default);

				if (_subscriptions > 0) // _subscriptions > 0: Do not 'ToImmutable' if no subscribers
				{
					_fullUpdates.OnNext(ToImmutable(topic, value, isRetained));
				}

				Parent?.OnChildUpdated(topic, value, isRetained);
			}
		}
		#endregion

		private MqttTopicValues ToImmutable()
		{
			// For now, we support only local and one child level of values

			var localValue = _localValue;
			var childrenValues = _children
				.Values
				.Where(child => child._hasLocalValue)
				.Select(child => (child, child._localValue)) as IEnumerable<(MqttCacheTopic topic, string value)>;

			// Here we make sure to propagate the updated value, even if it was not retained
			return new MqttTopicValues(
				Topic,
				localValue,
				childrenValues.ToImmutableDictionary(c => c.topic.Level, c => c.value),
				HasValue);
		}

		/// <summary>
		/// Captures the values of this topic into an immutable entity
		/// </summary>
		/// <param name="changedTopic"></param>
		/// <param name="changedValue"></param>
		/// <param name="isRetained"></param>
		/// <returns></returns>
		private MqttTopicValues ToImmutable(MqttCacheTopic changedTopic, string changedValue, bool isRetained)
		{
			// For now, we support only local and one child level of values

			var localValue = _localValue;
			var childrenValues = _children
				.Values
				.Where(child => child._hasLocalValue)
				.Select(child => (child, child._localValue)) as IEnumerable<(MqttCacheTopic topic, string value)>;

			// Here we make sure to propagate the updated value, even if it was not retained
			if (changedTopic == this)
			{
				localValue = changedValue;
			}
			else if (changedTopic?.Parent == this)
			{
				childrenValues = childrenValues
					.Where(child => child.topic != changedTopic)
					.Concat(new [] {(changedTopic, changedValue)});
			}

			return new MqttTopicValues(
				Topic, 
				localValue, 
				childrenValues.ToImmutableDictionary(c => c.topic.Level, c => c.value),
				isRetained);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription.Dispose();
			_children.Values.DisposeAllOrLog("Failed to dispose a sub topic of " + Topic);
		}
	}
}