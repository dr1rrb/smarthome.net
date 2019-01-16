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
			=> GetOrCreate(deviceId).AsDynamic();

		public HomeDevice<TDevice> GetDevice<TDevice>(string deviceId)
			where TDevice : IDeviceAdapter, new()
			=> GetOrCreate(deviceId).As<TDevice>();

		private Device GetOrCreate(string deviceId)
		{
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
			=> Interlocked.Exchange(ref _devices, null)?.Values.DisposeAllOrLog("Failed to dispoe a device");

		private class Device : IDisposable, IDevice
		{
			private readonly IScheduler _scheduler;
			private readonly IObservable<ExpandoObject> _source;

			private ImmutableDictionary<Type, IDisposable> _casts = ImmutableDictionary<Type, IDisposable>.Empty;

			/// <inheritdoc />
			public string Id { get; }

			public Device(string id, IDeviceHost host)
			{
				Id = id;
				_scheduler = host.Scheduler;
				_source = host
					.GetAndObserveState(this)
					.Select(changes =>
					{
						// Clone it to an expendo object
						var device = new ExpandoObject();
						var deviceValues = device as IDictionary<string, object>;
						foreach (var property in changes.Properties)
						{
							deviceValues[property.Key] = property.Value;
						}

						return device;
					})
					.Retry(TimeSpan.FromSeconds(10), _scheduler)
					.Publish()
					.RefCount();
			}

			public HomeDevice<ExpandoObject> AsDynamic()
			{
				return (HomeDevice<ExpandoObject>)ImmutableInterlocked.GetOrAdd(ref _casts, typeof(ExpandoObject), Create);

				IDisposable Create(Type _)
				{
					return new HomeDevice<ExpandoObject>(Id, _source);
				}
			}

			public HomeDevice<TDevice> As<TDevice>()
				where TDevice : IDeviceAdapter, new()
			{
				return (HomeDevice<TDevice>) ImmutableInterlocked.GetOrAdd(ref _casts, typeof(TDevice), Create);

				IDisposable Create(Type _)
				{
					var src = _source
						.Select(values =>
						{
							var device = new TDevice();
							device.Init(Id, values);
							return device;
						})
						.Retry(TimeSpan.FromSeconds(10), _scheduler);

					return new HomeDevice<TDevice>(Id, src);
				}
			}

			/// <inheritdoc />
			public void Dispose()
				=> Interlocked.Exchange(ref _casts, null)?.Values.DisposeAllOrLog("Failed to dispoe a casted device");
		}
	}
}