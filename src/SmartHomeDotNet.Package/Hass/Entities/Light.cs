using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public interface ILight : ISwitch, IDevice<ILight>
	{
	}
}