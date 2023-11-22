using System;
using System.Linq;

namespace Mavri.Ha.Api;

public class GetStatesCommand : HomeAssistantCommand
{
	public string Type => "get_states";
}