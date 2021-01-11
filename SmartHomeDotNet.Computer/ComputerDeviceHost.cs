using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer
{
	public class ComputerDeviceHost : IDeviceHost
	{
		private ImmutableDictionary<ComputerDeviceId, IComputerDevice> _devices = ImmutableDictionary<ComputerDeviceId, IComputerDevice>.Empty;

		public ComputerDeviceHost(IScheduler scheduler)
		{
			Scheduler = scheduler;
		}

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public object GetId(object rawId)
			=> Enum.Parse(typeof(ComputerDeviceId), rawId?.ToString(), true);

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState(IDevice device)
			=> Get((ComputerDeviceId) device.Id).GetAndObserveState();

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IDevice device)
			=> Get((ComputerDeviceId) device.Id).Execute(command);

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
			=> AsyncContextOperation.WhenAll(devices.Select(d => Execute(command, d)));

		private IComputerDevice Get(ComputerDeviceId device)
			=> ImmutableInterlocked.GetOrAdd(ref _devices, device, Create);

		private IComputerDevice Create(ComputerDeviceId device)
		{
			return device switch
			{
				ComputerDeviceId.Screen => new ScreenDevice(),
				ComputerDeviceId.Power => new PowerDevice(),
				_ => throw new ArgumentOutOfRangeException(nameof(device), $"Device '{device}' is not supported.")
			};
		}
	}
}
