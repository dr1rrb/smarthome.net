using SmartHomeDotNet.Hass.Api;
using System;
using System.Linq;
using System.Text.Json;

namespace SmartHomeDotNet.Hass.Api
{
	public record HomeAssistantEvent(string Type, DateTimeOffset Time, JsonElement Data)
	{
		public T GetData<T>()
			=> Data.Deserialize<T>(HomeAssistantWebSocketApi.JsonReadOpts);
	}
}
