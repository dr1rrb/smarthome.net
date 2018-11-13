using System;
using System.Collections.Generic;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Services
{
	public static class Lights
	{
		public static HomeAssistantApi.Call TurnOff(this HomeAssistantApi ha, params IDevice<ILight>[] lights)
			=> ha.Execute("light", "turn_off", new Dictionary<string, object> { { "entity_id", lights.Select(l => l.Id.Substring("homeassistant".Length).Trim('/').Replace('/', '.')).JoinBy(", ") } });
	}
}