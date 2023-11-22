using System;
using System.Linq;
using System.Text.Json;

namespace Mavri.Ha.Api;

public record HomeAssistantEvent(string Type, DateTimeOffset Time, JsonElement Data)
{
	public T? GetData<T>()
		=> Data.Deserialize<T>(HomeAssistantWebSocketApi.JsonReadOpts);
}