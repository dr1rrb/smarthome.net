using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// Represents a lazy holder of a remote device which will maintain device's state internally for fast access.
	/// </summary>
	/// <typeparam name="TDevice">The type of the device</typeparam>
	public sealed class HomeDevice<TDevice> : IObservable<TDevice>, IDisposable, IDevice<TDevice>
	{
		// The throttle to wait for the initial device to load before being published
		// It's frequent (eg. Home Assistant) to be dispatched on multiple MQTT topics, so with this delay we make sure
		// we have received all the updates of the device before publishing its value.
		// Note: We don't want to package this in the MqttDeviceHost (or any other device host) as we want to be able to 
		//		 apply this delay only when this HomeDevice is used using the Awaiter.
		private static readonly TimeSpan _initialThrottling = TimeSpan.FromMilliseconds(50);

		private readonly IConnectableObservable<TDevice> _source;

		private readonly Subject<TDevice> _device = new Subject<TDevice>();
		private TDevice _lastPersisted;
		private bool _hasPersisted;

		private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();

		private int _isConnected;

		public HomeDevice(IDeviceHost host, object id, IObservable<DeviceState> source, Func<DeviceState, IDeviceHost, TDevice> toDevice, IScheduler scheduler)
		{
			Host = host;
			Id = id;

			// Here we make sure to cache only the state that are flagged as "persisted" (a.k.a. 'retain' in MQTT)
			// so we will not replay a "transient" values (e.g. events like button pressed)
			_source = source
				.Select(state =>
				{
					var device = toDevice(state, Host);
					if (state.IsPersistedState)
					{
						_lastPersisted = device;
						_hasPersisted = true;
					}

					return device;
				})
				.Retry(Constants.DefaultRetryDelay, scheduler) // This will retry only for the "toDevice" as the source is already retried
				.Multicast(_device);
		}

		/// <summary>
		/// Gets the host of this device (for testing purposes)
		/// </summary>
		public IDeviceHost Host { get; }

		/// <inheritdoc />
		public object Id { get; }

		public Awaiter GetAwaiter()
		{
			Connect();

			return new Awaiter(this);
		}

		/// <inheritdoc />
		public IDisposable Subscribe(IObserver<TDevice> observer)
		{
			if (_hasPersisted)
			{
				observer.OnNext(_lastPersisted);
			}

			var subscription = _source.Subscribe(observer);

			Connect();

			return subscription;
		}

		private void Connect()
		{
			if (Interlocked.CompareExchange(ref _isConnected, 1, 0) == 0)
			{
				_subscription.Disposable = _source.Connect();
			}
		}

		/// <inheritdoc />
		public void Dispose()
			=> _subscription.Dispose();

		/// <summary>
		/// An async/await pattern awaiter optimized for the HomeDevice
		/// </summary>
		public struct Awaiter : INotifyCompletion
		{
			private readonly HomeDevice<TDevice> _owner;

			private TaskAwaiter<TDevice> _awaiter;
			private int _awaiterState;

			private static class States
			{
				public const int None = 0;
				public const int Initializing = 1;
				public const int Ready = 2;
			}

			public Awaiter(HomeDevice<TDevice> device)
			{
				_owner = device;
				_awaiter = default;
				_awaiterState = States.None;
			}

			public bool IsCompleted => _owner._hasPersisted || (_awaiterState == States.Ready && _awaiter.IsCompleted);

			public TDevice GetResult()
				=> _owner._hasPersisted 
					? _owner._lastPersisted
					: GetOrCreateAwaiter().GetResult();

			public void OnCompleted(Action continuation)
			{
				if (IsCompleted)
				{
					continuation();
				}
				else
				{
					GetOrCreateAwaiter().OnCompleted(continuation);
				}
			}

			private TaskAwaiter<TDevice> GetOrCreateAwaiter()
			{
				while (_awaiterState != States.Ready)
				{
					if (Interlocked.CompareExchange(ref _awaiterState, States.None, States.Initializing) == States.None)
					{
						try
						{
							_awaiter = _owner
								._device
								.Throttle(_initialThrottling)
								.FirstAsync()
								.ToTask(AsyncContext.CurrentToken)
								.GetAwaiter();
							_awaiterState = States.Ready;
							return _awaiter;
						}
						catch
						{
							_awaiterState = States.None;
							throw;
						}
					}
					else
					{
						Thread.SpinWait(3);
					}
				}

				return _awaiter;
			}
		}
	}
}