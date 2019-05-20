using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
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
		private static readonly TimeSpan _initialThrottling = TimeSpan.FromMilliseconds(50);

		private readonly IConnectableObservable<TDevice> _source;

		private readonly Subject<TDevice> _device = new Subject<TDevice>();
		private TDevice _lastPersistedDevice;
		private bool _hasPersistedDevice;

		private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();

		private int _isConnected;

		public HomeDevice(IDeviceHost host, string id, IObservable<DeviceState> source, Func<DeviceState, TDevice> toDevice, IScheduler scheduler)
		{
			Host = host;
			Id = id;

			// Here we make sure to cache only the state that are flagged as "persisted" (a.k.a. 'retain' in MQTT)
			// so we will not replay a "transient" values (e.g. events like button pressed)
			_source = source
				.Select(state =>
				{
					var device = toDevice(state);
					if (state.IsPersistedState)
					{
						_lastPersistedDevice = device;
						_hasPersistedDevice = true;
					}

					return device;
				})
				.Retry(Constants.DefaultRetryDelay, scheduler) // This will retry only for the "toDevice" as the source is already retried
				.Multicast(_device);
		}

		/// <summary>
		/// Gets the host of this device (for testing purposes)
		/// </summary>
		internal IDeviceHost Host { get; }

		/// <inheritdoc />
		public string Id { get; }

		public Awaiter GetAwaiter()
		{
			Connect();

			return new Awaiter(this);
		}

		/// <inheritdoc />
		public IDisposable Subscribe(IObserver<TDevice> observer)
		{
			if (_hasPersistedDevice)
			{
				observer.OnNext(_lastPersistedDevice);
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
		/// An async/await pattern awaiter optimized for the LazyDevice
		/// </summary>
		public struct Awaiter : INotifyCompletion
		{
			private readonly HomeDevice<TDevice> _owner;
			private TaskAwaiter<TDevice>? _awaiter;

			public Awaiter(HomeDevice<TDevice> device)
			{
				_owner = device;
				_awaiter = null;
			}

			public bool IsCompleted => _owner._hasPersistedDevice;

			public TDevice GetResult()
				=> _owner._hasPersistedDevice
					? _owner._lastPersistedDevice
					: throw new InvalidOperationException("This awaiter cannot run synchronously.");

			public async void OnCompleted(Action continuation)
			{
				if (_owner._hasPersistedDevice)
				{
					continuation();
				}
				else
				{
					if (_awaiter == null)
					{
						_awaiter = _owner
							._device
							.Throttle(_initialThrottling)
							.FirstAsync()
							.ToTask(AsyncContext.CurrentToken)
							.GetAwaiter();
					}

					_awaiter.Value.OnCompleted(() => continuation());
				}
			}
		}
	}
}