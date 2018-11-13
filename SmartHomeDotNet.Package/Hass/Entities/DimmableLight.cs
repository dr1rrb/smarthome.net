using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Entities
{
	public class DimmableLight : Switch, ILight
	{
		public double Brightness => int.Parse(Value.level) / 255.0;
	}
}