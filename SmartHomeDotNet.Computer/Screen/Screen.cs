using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Computer
{
	public class Screen : Device, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
	{
		public bool IsOn => TryGetValue("state", out var isOnStr) 
			&& bool.TryParse(isOnStr, out var isOn) 
			&& isOn;
	}
}