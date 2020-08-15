using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public class Cover : Device
	{
		public bool IsOpen => Raw.state == "open";

		public bool IsRunning => bool.Parse(Raw.running);

		public double Position => double.Parse(Raw.position) / 100;
	}
}