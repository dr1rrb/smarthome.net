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

		public static AsyncContextOperation Toggle<T>(this IDevice<T> device)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new Toggle(), device);

		public static AsyncContextOperation ApplyTo<TCommand>(this TCommand command, params IDevice<ISupport<TCommand>>[] devices)
			where TCommand : ICommand
			=> AsyncContextOperation.WhenAll(devices.GroupBy(d => d.Host).Select(g => g.Key.Execute(command, g)));
	}
}