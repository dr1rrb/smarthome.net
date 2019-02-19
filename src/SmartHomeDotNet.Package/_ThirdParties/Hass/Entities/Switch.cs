using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="Switch"/> device which allows type inference
	/// </summary>
	public interface ISwitch : IDevice<ISwitch>
	{
	}

	/// <summary>
	/// A device which can interact with the Switch component: <seealso cref="https://www.home-assistant.io/components/switch/"/>
	/// </summary>
	public class Switch : Device, ISwitch
	{
		/// <summary>
		/// Gets a boolean which indicates if the switch is on or not.
		/// </summary>
		public bool IsOn => Raw.state == "on";
	}
}