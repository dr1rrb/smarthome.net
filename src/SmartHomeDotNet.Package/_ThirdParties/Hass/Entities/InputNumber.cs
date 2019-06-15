using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="InputNumber"/> device which allows type inference
	/// </summary>
	public interface IInputNumber : IDevice<IInputNumber>
	{
	}

	/// <summary>
	/// A device for <seealso cref="https://www.home-assistant.io/components/input_number/"/>
	/// </summary>
	public class InputNumber : Device, IInputNumber
	{
		/// <summary>
		/// Gets the defined value
		/// </summary>
		public double Value => double.Parse(Raw.state);

		public static implicit operator double(InputNumber input)
			=> input.Value;
	}
}