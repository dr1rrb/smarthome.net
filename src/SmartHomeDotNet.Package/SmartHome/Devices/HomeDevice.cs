using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices
{
	//public abstract class AsyncDevice<TDevice> :/* IObservable<TDevice>,*/ IDisposable, IDevice<TDevice>
	//{
	//	/// <inheritdoc />
	//	public abstract object Id { get; }

	//	/// <inheritdoc />
	//	public abstract IDeviceHost Host { get; }

	//	protected abstract bool HasState { get; }

	//	protected abstract TDevice State { get; }

	//	protected abstract TaskAwaiter GetAwaiterImpl();

	//	public AsyncDeviceAwaiter GetAwaiter() => new AsyncDeviceAwaiter(this);

	//	public struct AsyncDeviceAwaiter : INotifyCompletion
	//	{
	//		private readonly AsyncDevice<TDevice> _owner;
	//		private TaskAwaiter? _awaiter;

	//		public AsyncDeviceAwaiter(AsyncDevice<TDevice> owner)
	//		{
	//			_owner = owner;
	//			_awaiter = null;
	//		}

	//		public bool IsCompleted => _owner.HasState;

	//		public TDevice GetResult()
	//			=> _owner.HasState
	//				? _owner.State
	//				: throw new InvalidOperationException("This awaiter cannot run synchronously.");

	//		public void OnCompleted(Action continuation)
	//		{
	//			if (_owner.HasState)
	//			{
	//				continuation();
	//			}
	//			else
	//			{
	//				if (_awaiter == null)
	//				{
	//					_awaiter = _owner.GetAwaiterImpl();
	//				}

	//				_awaiter.Value.OnCompleted(continuation);
	//			}
	//		}
	//	}

	//	/// <inheritdoc />
	//	public abstract void Dispose();
	//}

	/// <summary>
	/// Represents a lazy holder of a remote device which will maintain device's state internally for fast access.
	/// </summary>
	/// <typeparam name="TDevice">The type of the device</typeparam>
	public abstract class HomeDevice<TDevice> : IObservable<TDevice>, IDisposable, IDevice<TDevice>
	{
		// The throttle to wait for the initial device to load before being published
		// It's frequent (eg. Home Assistant) to be dispatched on multiple MQTT topics, so with this delay we make sure
		// we have received all the updates of the device before publishing its value.
		// Note: We don't want to package this in the MqqtDeviceHost (or any other device host) as we want to be able to 
		//		 apply this delay only when this HomeDevice is used using the Awaiter.
		private static readonly TimeSpan _initialThrottling = TimeSpan.FromMilliseconds(50);

		private static class State
		{
			public const int New = 0;
			public const int Initializing = 1;
			public const int Initialized = 2;
			public const int Connected = 3;
			public const int Disposed = int.MaxValue;
		}

		private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();
		private readonly IConnectableObservable<TDevice> _source;
		private readonly Subject<TDevice> _device = new Subject<TDevice>();

		private TDevice _lastPersisted;
		private bool _hasPersisted;

		//public HomeDevice(IDeviceHost host, object id, IObservable<DeviceState> source, Func<DeviceState, IDeviceHost, TDevice> toDevice, IScheduler scheduler)
		//{
		//	Host = host;
		//	Id = id;

		//	// Here we make sure to cache only the state that are flagged as "persisted" (a.k.a. 'retain' in MQTT)
		//	// so we will not replay a "transient" values (e.g. events like button pressed)
		//	_source = source
		//		.Select(state =>
		//		{
		//			var device = toDevice(state, Host);
		//			if (state.IsPersisted)
		//			{
		//				_lastPersisted = device;
		//				_hasPersisted = true;
		//			}

		//			return device;
		//		})
		//		.Retry(Constants.DefaultRetryDelay, scheduler) // This will retry only for the "toDevice" as the source is already retried
		//		.Multicast(_device);
		//}

		//public HomeDevice(IDeviceHost host, object id, IObservable<(TDevice value, bool isPersistent)> source, IScheduler scheduler)
		//{
		//	Host = host ?? throw new ArgumentNullException(nameof(host));
		//	Id = id ?? throw new ArgumentNullException(nameof(id));

		//	// Here we make sure to cache only the state that are flagged as "persisted" (a.k.a. 'retain' in MQTT)
		//	// so we will not replay a "transient" values (e.g. events like button pressed)
		//	_source = source
		//		.Do(state =>
		//		{
		//			if (state.value is ILazyDevice lazy)
		//			{
		//				lazy.TryInit(Id, Host);
		//			}

		//			if (state.value is IDevice dev)
		//			{
		//				if (dev.Id != Id)
		//				{
		//					this.Log().Error("The ID of the device state does not match the ID of the HomeDevice");
		//				}
		//				if (dev.Host != Host)
		//				{
		//					this.Log().Error("The Host of the device state does not match the Host of the HomeDevice");
		//				}
		//			}
		//		})
		//		.Select(state =>
		//		{
		//			if (state.isPersistent)
		//			{
		//				_lastPersisted = state.value;
		//				_hasPersisted = true;
		//			}

		//			return state.value;
		//		})
		//		.Retry(Constants.DefaultRetryDelay, scheduler) // This will retry only for the "toDevice" as the source is already retried
		//		.Multicast(_device);
		//}

		public HomeDevice()
		{
			// Here we make sure to cache only the state that are flagged as "persisted" (a.k.a. 'retain' in MQTT)
			// so we will not replay a "transient" values (e.g. events like button pressed)
			_source = Observable
				.Defer(() =>
				{
					return GetAndObserveState()
						.Do(state =>
						{
							if (state.value is ILazyDevice lazy)
							{
								lazy.TryInit(Id, Host);
							}

							if (state.value is IDevice dev)
							{
								if (dev.Id != Id)
								{
									this.Log().Error("The ID of the device state does not match the ID of the HomeDevice");
								}

								if (dev.Host != Host)
								{
									this.Log().Error("The Host of the device state does not match the Host of the HomeDevice");
								}
							}
						})
						.Select(state =>
						{
							if (state.isPersistent)
							{
								_lastPersisted = state.value;
								_hasPersisted = true;
							}

							return state.value;
						})
						.Retry(Constants.DefaultRetryDelay, Host.Scheduler); // This will retry only for the "toDevice" as the source is already retried
				})
				.Multicast(_device);
		}

		protected abstract IObservable<(TDevice value, bool isPersistent)> GetAndObserveState();

		/// <summary>
		/// Gets the host of this device (for testing purposes)
		/// </summary>
		public IDeviceHost Host => _host;

		/// <inheritdoc />
		public object Id => _id;

		private int _state = State.New;
		private IDeviceHost _host;
		private object _id;

		/// <summary>
		/// Initialize this device with the required properties.
		/// This must be invoked before accessing any method on this device.
		/// </summary>
		/// <param name="host">The host which manages this device</param>
		/// <param name="id">The id of this device</param>
		public void Init(IDeviceHost host, object id)
		{
			host = host ?? throw new ArgumentNullException(nameof(host));
			id = id ?? throw new ArgumentNullException(nameof(id));

			switch (Interlocked.CompareExchange(ref _state, State.Initializing, State.New))
			{
				case State.New:
					_host = host;
					_id = id;
					_state = State.Initialized;
					break;

				case State.Initializing:
				case State.Initialized:
				case State.Connected:
					throw new InvalidOperationException($"The device '{id}' was already initialized, you can init a device only once.");

				case State.Disposed:
					throw new ObjectDisposedException(nameof(HomeDevice<TDevice>));
			}
		}

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
			try
			{
				Connect();
			}
			catch
			{
				subscription.Dispose();
				throw;
			}

			return subscription;
		}

		private void Connect()
		{
			switch (Interlocked.CompareExchange(ref _state, State.Connected, State.Initialized))
			{
				case State.New:
				case State.Initializing:
					throw new InvalidOperationException($"The device '{Id ?? "??"}' was not initialized yet. Make sure to fully init the container before using a device.");

				case State.Initialized:
					_subscription.Disposable = _source.Connect();
					break;

				//case State.Connected:
				//	return;

				case State.Disposed:
					throw new ObjectDisposedException(nameof(HomeDevice<TDevice>));
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_state = State.Disposed;
			_subscription.Dispose();
		}

		/// <summary>
		/// An async/await pattern awaiter optimized for the HomeDevice
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

			public bool IsCompleted => _owner._hasPersisted;

			public TDevice GetResult()
				=> _owner._hasPersisted
					? _owner._lastPersisted
					: throw new InvalidOperationException("This awaiter cannot run synchronously.");

			public void OnCompleted(Action continuation)
			{
				if (_owner._hasPersisted)
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