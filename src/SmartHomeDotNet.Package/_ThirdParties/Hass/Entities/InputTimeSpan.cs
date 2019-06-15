using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="InputTimeSpan"/> device which allows type inference
	/// </summary>
	public interface IInputTimeSpan : IDevice<IInputTimeSpan>
	{
	}

	/// <summary>
	/// A device for <seealso cref="https://www.home-assistant.io/components/input_datetime/"/>
	/// </summary>
	public class InputTimeSpan : Device, IInputTimeSpan
	{
		/// <summary>
		/// Gets the defined VALUE
		/// </summary>
		public TimeSpan Time => TimeSpan.FromSeconds(double.Parse(Raw.timestamp));

		public static implicit operator TimeSpan(InputTimeSpan input)
			=> input.Time;
	}
}