using System;
using System.Linq;

namespace Mavri.Ha.Api;

public class CallServiceCommand : HomeAssistantCommand
{
	public CallServiceCommand(Domain domain, string service, ServiceData data)
	{
		Domain = domain;
		Service = service;
		Data = data;
	}

	public CallServiceCommand(Domain domain, string service, object? data = null)
	{
		Domain = domain;
		Service = service;
		Data = data;
	}

	public string Type => "call_service";

	public Domain Domain { get; }

	public string Service { get; }

	public object? Data { get; }
}