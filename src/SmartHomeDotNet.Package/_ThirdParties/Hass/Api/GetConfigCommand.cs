using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SmartHomeDotNet.Hass.Api;

public class GetConfigCommand : HomeAssistantCommand
{
	public string Type => "get_config";
}