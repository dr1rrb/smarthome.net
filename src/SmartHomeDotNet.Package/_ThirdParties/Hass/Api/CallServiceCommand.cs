using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api
{
	public class CallServiceCommand : HomeAssistantCommand
	{
		public CallServiceCommand(Domain domain, string service, ServiceData data)
		{
			Domain = domain;
			Service = service;
			Data = data;
		}

		public CallServiceCommand(Domain domain, string service, object data = null)
		{
			Domain = domain;
			Service = service;
			Data = data;
		}

		public string Type => "call_service";

		public Domain Domain { get; }

		public string Service { get; }

		public object Data { get; }

		/// <summary>
		/// An optional duration of the effect of the command (cf. remarks)
		/// </summary>
		/// <remarks>This is useful when dimming a light, so the resulting async operation of the API will include this duration and can be easily awaited if needed</remarks>
		public TimeSpan? Transition { get; }
	}
}