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
	}
}