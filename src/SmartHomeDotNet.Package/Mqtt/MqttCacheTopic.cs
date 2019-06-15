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
		private static class State
		{
			public const int IsActive = 0;

			public const int IsInitializing = 1 << 1;
			public const int ValueChanged = 1 << 4;
			public const int ChildChanged = 1 << 5;

			public const int Disposed = int.MaxValue; // must have other flags set!
		}

		//private readonly Subject<(MqttCacheTopic topic, string value)> _updates = new Subject<(MqttCacheTopic, string)>();

		private readonly Subject<string> _localUpdates = new Subject<string>();
		private readonly Subject<MqttTopicValues> _fullUpdates = new Subject<MqttTopicValues>();
		private readonly Subject<Unit> _msgReceived = new Subject<Unit>();
		private readonly SerialDisposable _probing = new SerialDisposable();

		private readonly IMqttConnection _connection;

		private int _state;
		private int _subscriptions;
		private ImmutableDictionary<string, MqttCacheTopic> _children = ImmutableDictionary<string, MqttCacheTopic>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
		private bool _hasLocalValue;
		private string _localValue;

		/// <summary>
		/// The full topic path
		/// </summary>
		public string Topic { get; }

		/// <summary>
		/// The lats level of this topic
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
		}

		public void AddPermanentRootSubscription() 
			=> Interlocked.Increment(ref _subscriptions);

		/// <summary>
		/// Gets a create a child topic
		/// </summary>
		/// <param name="level">The name of the sub topic</param>
		/// <returns></returns>
		public MqttCacheTopic GetChild(string level)
			=> ImmutableInterlocked.GetOrAdd(ref _children, level, n => new MqttCacheTopic(_connection, this, n));

		#region Initialization
		/// <summary>
		/// Prevents this topic to publish its **retained** updates
		/// </summary>
		/// <remarks>The messages will however still be propagated through the topic structure</remarks>
		public void HoldUpdates()
		{
			// Abort any pending probing
			_probing.Disposable = Disposable.Empty;

			// Set the 'IsInitializing' flag to prevent publication of updates
			GoToInitializing();

			// Finally pause all sub topics
			foreach (var child in _children.Values)
			{
				child.HoldUpdates();
			}
		}

		/// <summary>
		/// Publish the updates that was buffer while <see cref="HoldUpdates"/>.
		/// </summary>
		public void FlushUpdates()
		{
			// First re-enable all sub topics so they can start their probing
			foreach (var child in _children.Values)
			{
				child.FlushUpdates();
			}

			// Then request to start probing
			Probe(isReset: true);
		}

		private void Probe(bool isReset = false)
		{
			// Don't Subscribe to topic if no subscriptions active or if we already have value
			if (_subscriptions == 0 
				|| (!isReset && (HasValue || _state != State.IsActive)))
			{
				return;
			}

			// Set as initializing
			GoToInitializing();

			// Determine when does the full state has been loaded
			var isProbing = true;

			_probing.Disposable = Observable
				.DeferAsync(async ct =>
				{
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

					var notUpdatedSinceAFew = this
						._msgReceived
						.Throttle(TimeSpan.FromMilliseconds(100), _connection.Scheduler);
					var timeout = Observable
						.Timer(HasValue ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromSeconds(5), _connection.Scheduler)
						.Select(_ => Unit.Default);

					return notUpdatedSinceAFew.Amb(timeout);
				})
				.Materialize() // mute errors
				.FirstAsync()
				.Finally(() => isProbing = false)
				.Subscribe(_ => GoToActive(ref isProbing));

		}

		private void GoToInitializing()
		{
			int state;
			do
			{
				state = _state;
				if (state == State.Disposed)
				{
					throw new ObjectDisposedException(nameof(MqttCacheTopic));
				}
			} while (Interlocked.CompareExchange(ref _state, State.IsInitializing, state) != state);
		}

		private void GoToActive(ref bool isProbing)
		{
			do
			{
				var state = _state;
				if (state == State.Disposed)
				{
					return;
				}
				else if (Interlocked.CompareExchange(ref _state, State.IsActive, state) == state)
				{
					var valueChanged = (state & State.ValueChanged) == State.ValueChanged;
					var childChanged = (state & State.ChildChanged) == State.ChildChanged;

					if (_subscriptions > 0)
					{
						if (valueChanged)
						{
							_localUpdates.OnNext(_localValue);
							_fullUpdates.OnNext(ToImmutable());
						}
						else if (childChanged)
						{
							_fullUpdates.OnNext(ToImmutable());
						}
					}

					if (valueChanged || childChanged)
					{
						Parent?.OnChildUpdated(this, _localValue, isRetained: true);
					}

					return;
				}

				//else if (
				//	(state & State.ValueChanged) == State.ValueChanged
				//	&& Interlocked.CompareExchange(ref _concurrentState, State.IsActive, state) == state)
				//{
				//	// Successfully removed the "value changed" flag, so publish the updates
				//	if (_subscriptions > 0)
				//	{
				//		_localUpdates.OnNext(_localValue);
				//		_fullUpdates.OnNext(ToImmutable());
				//	}

				//	Parent?.OnChildUpdated(this, _localValue, isRetained: true);
				//}
				//else if (
				//	(state & State.ChildChanged) == State.ChildChanged
				//	&& Interlocked.CompareExchange(ref _concurrentState, State.IsActive, state) == state)
				//{
				//	// Successfully removed the "child changed" flag, so publish the update
				//	if (_subscriptions > 0)
				//	{
				//		_fullUpdates.OnNext(ToImmutable());
				//	}

				//	Parent?.OnChildUpdated(this, _localValue, isRetained: true);
				//}
				//else if (Interlocked.CompareExchange(ref _concurrentState, State.IsActive, state) == state)
				//{
				//	// We successfully re-activated this topic
				//	return;
				//}
			} while (isProbing);
		}
		#endregion

		#region [GetAnd]Observe topic values
		public IObservable<MqttTopicValues> GetAndObserve() => Observable.Defer(() =>
		{
			Interlocked.Increment(ref _subscriptions);
			Probe();

			var src = _fullUpdates.Finally(() => Interlocked.Decrement(ref _subscriptions));

			return _state == State.IsActive && HasValue
				? src.StartWith(Scheduler.Immediate, ToImmutable())
				: src;
		});

		public IObservable<MqttTopicValues> Observe() => Observable.Defer(() =>
		{
			Interlocked.Increment(ref _subscriptions);
			Probe();

			return _fullUpdates.Finally(() => Interlocked.Decrement(ref _subscriptions));
		});

		public IObservable<string> GetAndObserveLocalValue() => Observable.Defer(() =>
		{
			Interlocked.Increment(ref _subscriptions);
			Probe();

			var src = _localUpdates.Finally(() => Interlocked.Decrement(ref _subscriptions));

			return _state == State.IsActive && _hasLocalValue
				? src.StartWith(Scheduler.Immediate, _localValue)
				: src;
		});

		public IObservable<string> ObserveLocalValue() => Observable.Defer(() =>
		{
			Interlocked.Increment(ref _subscriptions);
			Probe();

			return _localUpdates.Finally(() => Interlocked.Decrement(ref _subscriptions));
		});
		#endregion

		#region Update
		public bool TryUpdate(string value, bool retained)
		{
			// If we already had a retained value (stored as local), we are still persist it, no matter the 'retained' flag
			// However, I'm not sure if this is really required nor even a good idea ...
			// Note: This is needed as HA seems to not publish updates as retained (missing config?)
			retained |= _hasLocalValue;

			if (!retained)
			{
				//_updates.OnNext((this, value));
				//Parent?.ChildUpdated(this, value, retained);
				OnUpdated(value, retained);

				return true;
			}
			else if (_hasLocalValue && (_localValue?.Equals(value, StringComparison.InvariantCultureIgnoreCase) ?? value == null))
			{
				return false;
			}
			else
			{
				_localValue = value;
				_hasLocalValue = true;

				//_updates.OnNext((this, value));
				//Parent?.ChildUpdated(this, value, retained);
				OnUpdated(value, retained);

				return true;
			}
		}

		private void OnUpdated(string value, bool isRetained)
		{
			//while ((_concurrentState & (State.IsInitalizing | State.ValueChanged)) != 0)
			//{
			//	var current = _concurrentState;
			//	if ((current & State.ValueChanged) != State.ValueChanged)
			//	{
			//		// Well, either we are disposed or the "value changed" flag has already been set, nothing to do!
			//		return;
			//	}

			//	var updated = current | State.ValueChanged;
			//	if (Interlocked.CompareExchange(ref _concurrentState, updated, current) == current)
			//	{
			//		// We successfully set the "value changed" flag, nothing to do else.
			//		return;
			//	}
			//}

			//if (_state.Value != State.Active)
			//{
			//	lock (_state)
			//	{
			//		if (_state.Value != State.Active)
			//		{

			//			return;
			//		}
			//	}


			bool canPublish;
			while (
				!(canPublish = (_state & (State.IsInitializing | State.ValueChanged)) == 0)
				&& isRetained)
			{
				// Something prevent us to publish the **state** (i.e. !isRetained) updated

				var state = _state;

				if (state == State.Disposed)
					return; // We are disposed, abort.

				if ((state & State.ValueChanged) == State.ValueChanged)
					break; // The "value changed" flag has already been set, continue with canPublish == false

				if (Interlocked.CompareExchange(ref _state, state | State.ValueChanged, state) == state)
					break; // We successfully set the "value changed" flag, continue with canPublish == false
			}

			_msgReceived.OnNext(Unit.Default);

			if (canPublish && _subscriptions > 0) // Do not 'ToImmutable' if no subscribers
			{
				_localUpdates.OnNext(value);
				_fullUpdates.OnNext(ToImmutable(this, value, isRetained));
			}

			Parent?.OnChildUpdated(this, value, isRetained);
		}

		private void OnChildUpdated(MqttCacheTopic topic, string value, bool isRetained)
		{
			// First validate if we are allowed to publish this update
			//   1. If not retained (i.e. "event")
			//   2. 

			bool canPublish;
			while (
				!(canPublish = (_state & (State.IsInitializing | State.ChildChanged)) == 0)
				&& isRetained)
			{
				// Something prevent us to publish the **state** (i.e. !isRetained) updated

				var state = _state;

				if (state == State.Disposed)
					return; // We are disposed, abort.

				if ((state & State.ChildChanged) == State.ChildChanged)
					break; // The "child changed" flag has already been set, continue with canPublish == false

				if (Interlocked.CompareExchange(ref _state, state | State.ChildChanged, state) == state)
					break; // We successfully set the "child changed" flag, continue with canPublish == false
			}

			_msgReceived.OnNext(Unit.Default);

			if (canPublish && _subscriptions > 0) // _subscriptions > 0: Do not 'ToImmutable' if no subscribers
			{
				_fullUpdates.OnNext(ToImmutable(topic, value, isRetained));
			}

			Parent?.OnChildUpdated(topic, value, isRetained);
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
				isRetained
				// changedTopic == null
				// 	? HasValue // For the initial value we consider as retained only if we actually have any values
				// 	: changedTopic._localValue == changedValue // For changes notif, we only have to check if the value was effectively persisted or not
				);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_probing.Dispose();
			_children.Values.DisposeAllOrLog("Failed to dispose a sub topic of " + Topic);
		}
	}
}