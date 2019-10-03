using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.Hass.Commands;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="InputText"/> device which allows type inference
	/// </summary>
	public interface IInputText : IDevice<IInputText>, ISupport<SetText>
	{
	}

	/// <summary>
	/// A device for <seealso cref="https://www.home-assistant.io/components/input_text/"/>
	/// </summary>
	public class InputText : Device, IInputText
	{
		/// <summary>
		/// Gets the defined value
		/// </summary>
		public string Value => Raw.state;

		public static implicit operator string(InputText input)
			=> input.Value;
	}
}
