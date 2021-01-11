using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Computer
{
	public class Power : Device, ISupport<TurnOff>, ISupport<Sleep>
	{
	}
}