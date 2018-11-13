using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public interface ISwitch : IDevice<ISwitch>
	{
	}

	public class Switch : Device, ISwitch
	{
		public bool IsOn => Value.state == "on";
	}
}