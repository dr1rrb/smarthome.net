using System;
using SmartHomeDotNet.Hass.Commands;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public class Lock : Device, ISupport<Commands.Lock>, ISupport<Unlock>
	{
		public LockState State => Raw.state == "locked"
			? LockState.Locked
			: LockState.Unlocked;
	}
}
