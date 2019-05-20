using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices
{
	public sealed class HomeDevicesManager : IDisposable
	{
		private readonly IDeviceHost _host;

		private ImmutableDictionary<string, Device> _devices = ImmutableDictionary<string, Device>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);

		public HomeDevicesManager(IDeviceHost host)
		{
			_host = host;
		}

		public HomeDevice<ExpandoObject> GetDevice(string deviceId)
			=> GetOrCreate(deviceId).As(Device.Dynamic);

		public HomeDevice<TDevice> GetDevice<TDevice>(string deviceId)
			where TDevice : IDeviceAdapter, new()
			=> GetOrCreate(deviceId).As(Device.Generic<TDevice>);

		public HomeDevice<TDevice> GetDevice<TDevice>(string deviceId, Func<DeviceState, TDevice> deviceFactory)
			=> GetOrCreate(deviceId).As(deviceFactory);

		private Device GetOrCreate(string deviceId)
		{
			// As we use 'devices == null' to check the dispose state, we cannot use:
			// ImmutableInterlocked.GetOrAdd(ref _devices, deviceId, (id, host) => new Device(id, host), _host);
			var newDevice = default(Device);
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
					newDevice = new Device(deviceId, _host);
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

		private class Device : IDisposable, IDevice
		{
			public static TDevice Generic<TDevice>(DeviceState state)
				where TDevice : IDeviceAdapter, new()
			{
				var device = new TDevice();
				device.Init(state);
				return device;
			}

			public static ExpandoObject Dynamic(DeviceState state)
				=> state.ToDynamic();


			private readonly IDeviceHost _host;
			private readonly IScheduler _scheduler;
			private readonly IObservable<DeviceState> _source;

			private ImmutableDictionary<Type, IDisposable> _casts = ImmutableDictionary<Type, IDisposable>.Empty;

			/// <inheritdoc />
			public string Id { get; }

			public Device(string id, IDeviceHost host)
			{
				_host = host;
				Id = id;
				_scheduler = host.Scheduler;
				_source = host
					.GetAndObserveState(this)
					.Retry(Constants.DefaultRetryDelay, _scheduler)
					.Publish()
					.RefCount();
			}

			public HomeDevice<TDevice> As<TDevice>(Func<DeviceState, TDevice> factory)
			{
				return (HomeDevice<TDevice>)ImmutableInterlocked.GetOrAdd(ref _casts, typeof(TDevice), Create, factory);

				IDisposable Create(Type _, Func<DeviceState, TDevice> f) 
					=> new HomeDevice<TDevice>(_host, Id, _source, f, _scheduler);
			}

			/// <inheritdoc />
			public void Dispose()
				=> Interlocked.Exchange(ref _casts, null)?.Values.DisposeAllOrLog("Failed to dispoe a casted device");
		}
	}
}