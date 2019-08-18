using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Commands
{
	public static class CommandExtensions
	{
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(), device);

		public static AsyncContextOperation TurnOff<T>(this IDevice<T> device)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new TurnOff(), device);

		public static AsyncContextOperation Apply<TCommand>(this TCommand command, params IDevice<ISupport<TCommand>>[] devices)
			where TCommand : ICommand
		{
			foreach (var devicesPerHost in devices.GroupBy(d => d.Host))
			{
				devicesPerHost.Key.Execute(command, devicesPerHost);
			}

			return null;
		}
	}
}