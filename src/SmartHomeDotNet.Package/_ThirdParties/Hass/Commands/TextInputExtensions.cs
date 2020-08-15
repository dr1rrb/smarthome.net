using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	/// <summary>
	/// Device extensions for the <see cref="SetText"/> command
	/// </summary>
	public static class TextInputExtensions
	{
		/// <summary>
		/// Set the text value of this input text
		/// </summary>
		/// <param name="value">The value to set</param>
		public static AsyncContextOperation Set<T>(this IDevice<T> device, string value)
			where T : ISupport<SetText>
			=> device.Host.Execute(new SetText { Value = value }, device);
	}
}