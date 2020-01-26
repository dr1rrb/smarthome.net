using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SmartHomeDotNet.Hass.Events
{
	public class IosAction
	{
		public Guid ActionId { get; set; }

		public string ActionName { get; set; }

		[JsonPropertyName("sourceDevicePermanentID")]
		public Guid SourceDeviceUuid { get; set; }

		public string SourceDeviceId { get; set; }

		public string SourceDeviceName { get; set; }

		public string TriggerSource { get; set; }
	}
}