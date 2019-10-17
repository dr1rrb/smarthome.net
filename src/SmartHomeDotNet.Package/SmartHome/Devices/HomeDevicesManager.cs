using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices
{
	public interface IDeviceStateProvider<TDeviceId>
	{
		TDeviceId Parse(object rawId);

		IObservable<DeviceState> GetAndObserveState(TDeviceId id);

		//IObservable<TDevice> GetAndObserveState<TDevice>(TDeviceId id);
	}

	//public interface IDeviceActuator
	//{
	//	AsyncContextOperation Execute(ICommand command, IDevice device);

	//	AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices);
	//}

	public sealed class HomeDevicesManager<TDeviceId> : IDisposable
	{
		private readonly IDeviceHost _host;
		private readonly IDeviceStateProvider<TDeviceId> _stateProvider;

		private ImmutableDictionary<object, Holder> _devices = ImmutableDictionary<object, Holder>.Empty.WithComparers(EqualityComparer<object>.Default);

		public HomeDevicesManager(IDeviceHost host, IDeviceStateProvider<TDeviceId> stateProvider)
		{
			_host = host;
			_stateProvider = stateProvider;
		}

		public HomeDevice<ExpandoObject> GetDevice(object rawId)
			=> GetOrCreate(rawId).As(Holder.Dynamic);

		public HomeDevice<TDevice> GetDevice<TDevice>(object rawId)
			where TDevice : IDeviceAdapter, new()
			=> GetOrCreate(rawId).As(Holder.Generic<TDevice>);

		public HomeDevice<TDevice> GetDevice<TDevice>(object rawId, Func<DeviceState, IDeviceHost, TDevice> deviceFactory)
			=> GetOrCreate(rawId).As(deviceFactory);

		private Holder GetOrCreate(object rawId)
		{
			// As we use 'devices == null' to check the dispose state, we cannot use:
			// ImmutableInterlocked.GetOrAdd(ref _devices, deviceId, (id, host) => new Device(id, host), _host);
			var newDevice = default(Holder);
			var deviceId = _stateProvider.Parse(rawId);
			while (true)
			{
				var devices = _devices;
				if (devices == null)
				{
					throw new ObjectDisposedException(nameof(HomeAssistantApi));
				}

				if (devices.TryGetValue(deviceId, out var device))
				{
					return device;
				}

				if (newDevice == null)
				{
					newDevice = new Holder(this, deviceId);
				}

				if (Interlocked.CompareExchange(ref _devices, devices.Add(deviceId, newDevice), devices) == devices)
				{
					return newDevice;
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
			=> Interlocked.Exchange(ref _devices, null)?.Values.DisposeAllOrLog("Failed to dispose a device");

		private class Holder : IDisposable
		{
			public static TDevice Generic<TDevice>(DeviceState state, IDeviceHost host)
				where TDevice : IDeviceAdapter, new()
			{
				var device = new TDevice();
				device.Init(state, host);
				return device;
			}

			public static ExpandoObject Dynamic(DeviceState state, IDeviceHost host)
				=> state.ToDynamic();

			private readonly HomeDevicesManager<TDeviceId> _owner;
			private readonly TDeviceId _id;
			private readonly IObservable<DeviceState> _source;

			private ImmutableDictionary<Type, IDisposable> _casts = ImmutableDictionary<Type, IDisposable>.Empty;

			public Holder(HomeDevicesManager<TDeviceId> owner, TDeviceId id)
			{
				_owner = owner;
				_id = id;
				_source = owner
					._stateProvider
					.GetAndObserveState(id)
					.Retry(Constants.DefaultRetryDelay, _owner._host.Scheduler)
					.Publish()
					.RefCount();
			}

			public HomeDevice<TDevice> As<TDevice>(Func<DeviceState, IDeviceHost, TDevice> factory)
				=> (HomeDevice<TDevice>)ImmutableInterlocked.GetOrAdd(ref _casts, typeof(TDevice), (_, f) => new Dev<TDevice>(_owner._host, _id, _source, f), factory);

			/// <inheritdoc />
			public void Dispose()
				=> Interlocked.Exchange(ref _casts, null)?.Values.DisposeAllOrLog("Failed to dispose a casted device");
		}

		private sealed class Dev<TDevice> : HomeDevice<TDevice>
		{
			private readonly Func<DeviceState, IDeviceHost, TDevice> _toDevice;
			private readonly IObservable<DeviceState> _source;

			public Dev(IDeviceHost host, object id, IObservable<DeviceState> source, Func<DeviceState, IDeviceHost, TDevice> toDevice)
			{
				_source = source;
				_toDevice = toDevice;

				Init(host, id);
			}

			protected override IObservable<(TDevice value, bool isPersistent)> GetAndObserveState()
				=> _source.Select(state => (_toDevice(state, Host), state.IsPersisted));
		}
	}


	//public sealed class HomeDevicesManager : IDisposable
	//{
	//	private readonly IDeviceHost _host;

	//	private ImmutableDictionary<object, Holder> _devices = ImmutableDictionary<object, Holder>.Empty.WithComparers(EqualityComparer<object>.Default);

	//	public HomeDevicesManager(IDeviceHost host)
	//	{
	//		_host = host;
	//	}

	//	public HomeDevice<ExpandoObject> GetDevice(object deviceId)
	//		=> GetOrCreate(_host.GetId(deviceId)).As(Holder.Dynamic);

	//	public HomeDevice<TDevice> GetDevice<TDevice>(object deviceId)
	//		where TDevice : IDeviceAdapter, new()
	//		=> GetOrCreate(_host.GetId(deviceId)).As(Holder.Generic<TDevice>);

	//	public HomeDevice<TDevice> GetDevice<TDevice>(object deviceId, Func<DeviceState, IDeviceHost, TDevice> deviceFactory)
	//		=> GetOrCreate(_host.GetId(deviceId)).As(deviceFactory);

	//	private Holder GetOrCreate(object deviceId)
	//	{
	//		// As we use 'devices == null' to check the dispose state, we cannot use:
	//		// ImmutableInterlocked.GetOrAdd(ref _devices, deviceId, (id, host) => new Device(id, host), _host);
	//		var newDevice = default(Holder);
	//		while (true)
	//		{
	//			var devices = _devices;
	//			if (devices == null)
	//			{
	//				throw new ObjectDisposedException(nameof(HomeAssistantApi));
	//			}

	//			if (devices.TryGetValue(deviceId, out var device))
	//			{
	//				return device;
	//			}

	//			if (newDevice == null)
	//			{
	//				newDevice = new Holder(deviceId, _host);
	//			}

	//			if (Interlocked.CompareExchange(ref _devices, devices.Add(deviceId, newDevice), devices) == devices)
	//			{
	//				return newDevice;
	//			}
	//		}
	//	}

	//	/// <inheritdoc />
	//	public void Dispose()
	//		=> Interlocked.Exchange(ref _devices, null)?.Values.DisposeAllOrLog("Failed to dispose a device");

	//	private class Holder : IDisposable, IDevice
	//	{
	//		public static TDevice Generic<TDevice>(DeviceState state, IDeviceHost host)
	//			where TDevice : IDeviceAdapter, new()
	//		{
	//			var device = new TDevice();
	//			device.Init(state, host);
	//			return device;
	//		}

	//		public static ExpandoObject Dynamic(DeviceState state, IDeviceHost host)
	//			=> state.ToDynamic();

	//		private readonly IScheduler _scheduler;
	//		private readonly IObservable<DeviceState> _source;

	//		private ImmutableDictionary<Type, IDisposable> _casts = ImmutableDictionary<Type, IDisposable>.Empty;

	//		/// <inheritdoc />
	//		public object Id { get; }

	//		/// <inheritdoc />
	//		public IDeviceHost Host { get; }

	//		public Holder(object id, IDeviceHost host)
	//		{
	//			Id = id;
	//			Host = host;

	//			_scheduler = host.Scheduler;
	//			_source = host
	//				.GetAndObserveState(this)
	//				.Retry(Constants.DefaultRetryDelay, _scheduler)
	//				.Publish()
	//				.RefCount();
	//		}

	//		public HomeDevice<TDevice> As<TDevice>(Func<DeviceState, IDeviceHost, TDevice> factory)
	//		{
	//			return (HomeDevice<TDevice>)ImmutableInterlocked.GetOrAdd(ref _casts, typeof(TDevice), Create, factory);

	//			IDisposable Create(Type _, Func<DeviceState, IDeviceHost, TDevice> f) 
	//				=> new HomeDevice<TDevice>(Host, Id, _source, f, _scheduler);
	//		}

	//		/// <inheritdoc />
	//		public void Dispose()
	//			=> Interlocked.Exchange(ref _casts, null)?.Values.DisposeAllOrLog("Failed to dispose a casted device");
	//	}
	//}
}