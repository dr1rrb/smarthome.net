using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public interface ILight : IDevice<ILight>
	{
	}

	public class Light : Device, ILight
	{
		public bool IsOn => Value.state == "on";
	}
}