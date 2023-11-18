using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api;

public class GenericCommand : HomeAssistantCommand
{
	public string Type { get; }

	public GenericCommand(string type)
	{
		Type = type;
	}
}