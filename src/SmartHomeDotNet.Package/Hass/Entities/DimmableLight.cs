using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public interface IDimmableLight : IDevice<IDimmableLight>
	{
	}

	public class DimmableLight : Light, IDimmableLight
	{
		public double Brightness => int.Parse(Value.brightness) / 255.0;
	}
}