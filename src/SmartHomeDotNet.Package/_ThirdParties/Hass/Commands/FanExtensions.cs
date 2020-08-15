using System;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	/// <summary>
	/// Device extensions for the <see cref="SetSpeed"/> command
	/// </summary>
	public static class FanExtensions
	{
		/// <summary>
		/// Sets the speed of one this fan
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation SetSpeed<T>(this IDevice<T> device, Fan.Speeds speed)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new SetSpeed(speed), device);
	}
}