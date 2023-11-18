using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SmartHomeDotNet.Hass.Api;

public abstract class HomeAssistantCommand
{
	[JsonConverter(typeof(CommandIdInjector))]
	public int Id { get; } = -1;
}