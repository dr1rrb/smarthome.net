using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	public static class LockExtensions
	{
		public static AsyncContextOperation Lock<T>(this IDevice<T> device, string code = null)
			where T : ISupport<Lock>
			=> device.Host.Execute(new Lock(code), device);

		public static AsyncContextOperation Unlock<T>(this IDevice<T> device, string code = null)
			where T : ISupport<Lock>
			=> device.Host.Execute(new Unlock(code), device);
	}
}