using System;
using System.Collections.Generic;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Services
{
	public static class Switches
	{
		public static AsyncContextOperation TurnOff(this HomeAssistantApi ha, params IDevice<ISwitch>[] lights)
			=> ha.Execute("switch", "turn_off", new Dictionary<string, object>
			{
				{ "entity_id", lights.Select(l => l.Id).JoinBy(", ") }
			});

		public static AsyncContextOperation TurnOn(this HomeAssistantApi ha, params IDevice<ISwitch>[] lights)
			=> ha.Execute("switch", "turn_on", new Dictionary<string, object>
			{
				{ "entity_id", lights.Select(l => l.Id).JoinBy(", ") }
			});

		public static AsyncContextOperation Toggle(this HomeAssistantApi ha, params IDevice<ISwitch>[] lights)
			=> ha.Execute("switch", "toggle", new Dictionary<string, object>
			{
				{ "entity_id", lights.Select(l => l.Id).JoinBy(", ") }
			});
	}
}