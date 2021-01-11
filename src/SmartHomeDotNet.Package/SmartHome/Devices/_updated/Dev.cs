#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	public sealed class Dev<TState> : HomeDevice<TState> //IObservable<TState>, IDisposable, IDevice<TState>
		where TState: IDeviceState
	{
		public Dev(IDeviceHost host, object id, IObservable<TState> source, IScheduler scheduler)
			: base(new DeviceHostAdapter(host, scheduler), id, source, s => s.IsPersistent, scheduler)
		{
		}

		private class DeviceHostAdapter : Devices.IDeviceHost
		{
			private readonly IDeviceHost _inner;

			public DeviceHostAdapter(IDeviceHost inner, IScheduler scheduler)
			{
				_inner = inner;
				Scheduler = scheduler;
			}

			/// <inheritdoc />
			public IScheduler Scheduler { get; }

			/// <inheritdoc />
			public object GetId(object rawId)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public IObservable<DeviceState> GetAndObserveState(IDevice device)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public AsyncContextOperation Execute(ICommand command, IDevice device)
				=> _inner.Execute(command, device.Id);

			/// <inheritdoc />
			public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
				=> _inner.Execute(command, devices.Select(dev => dev.Id).ToArray());
		}
	}
}