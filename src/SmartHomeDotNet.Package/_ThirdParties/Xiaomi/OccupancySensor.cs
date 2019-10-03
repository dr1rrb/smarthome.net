using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Xiaomi.Devices
{
	public class OccupancySensor : Device
	{
		public bool IsOccupied => bool.Parse(Raw.occupancy);

		public int Illuminance => int.Parse(Raw.illuminance, CultureInfo.InvariantCulture);
	}
}
