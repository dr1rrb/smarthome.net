using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api;

public class GetStatesCommand : HomeAssistantCommand
{
	public string Type => "get_states";
}