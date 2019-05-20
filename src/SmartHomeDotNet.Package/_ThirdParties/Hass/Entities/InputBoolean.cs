using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="InputBoolean"/> device which allows type inference
	/// </summary>
	public interface IInputBoolean : IDevice<IInputBoolean>
	{
	}

	/// <summary>
	/// A device for <seealso cref="https://www.home-assistant.io/components/input_boolean/"/>
	/// </summary>
	public class InputBoolean : Device, IInputBoolean
	{
		/// <summary>
		/// Gets the current state of the input
		/// </summary>
		public bool Value => Raw.state == "on";

		public static implicit operator bool(InputBoolean input)
			=> input.Value;
	}
}