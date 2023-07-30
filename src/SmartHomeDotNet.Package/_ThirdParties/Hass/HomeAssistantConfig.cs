using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SmartHomeDotNet.Hass
{
	public class HomeAssistantConfig
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("unique_id")]
		public string Id { get; set; }

		[JsonProperty("state_topic")]
		public string StateTopic { get; set; }

		[JsonProperty("state_on")]
		public string StateOn { get; set; }

		[JsonProperty("state_off")]
		public string StateOff { get; set; }

		[JsonProperty("optimistic")]
		public bool IsOptimistic { get; set; }

		[JsonProperty("command_topic")]
		public string CommandTopic { get; set; }

		[JsonProperty("payload_on")]
		public string PayloadOn { get; set; }

		[JsonProperty("payload_off")]
		public string PayloadOff { get; set; }

		[JsonProperty("availability_topic")]
		public string AvailabilityTopic { get; set; }

		[JsonProperty("icon")]
		public string Icon { get; set; }

		[JsonProperty("retain")]
		public bool IsRetained { get; set; }

		[JsonProperty("device")]
		public HomeAssistantConfigDevice Device { get; set; }
	}

	public class HomeAssistantConfigDevice
	{
		public HomeAssistantConfigDevice(string identifier)
		{
			Identifiers = new List<string> {identifier};
		}

		[JsonProperty("identifiers")]
		public List<string> Identifiers { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("sw_version")]
		public string Firmware { get; set; }

		[JsonProperty("model")]
		public string Model { get; set; }

		[JsonProperty("manufacturer")]
		public string Manufacturer { get; set; }
	}
}