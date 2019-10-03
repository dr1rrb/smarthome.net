using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	/// <summary>
	/// Device extensions for the <see cref="Select{T}"/> command
	/// </summary>
	public static class SelectExtensions
	{
		/// <summary>
		/// Sets the given value to this device
		/// </summary>
		/// <param name="option">The value to set</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation Select<TDevice, TValue>(this IDevice<TDevice> device, TValue option)
			where TDevice : ISupport<Select<TValue>>
			=> device.Host.Execute(new Select<TValue>(option), device);
	}
}