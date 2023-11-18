using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SmartHomeDotNet.Hass.Events
{
	public class IosAction
	{
		/*
		 	event_type: ios.action_fired
			data:
			  actionID: 2F5A9414-3B41-4370-B85B-4A35DB5070EE
			  actionName: Sleep
			  sourceDeviceID: achlys
			  sourceDeviceName: Achlys
			  sourceDevicePermanentID: 0EEFFE8D-10F4-4660-BC5B-478D5D05043D
			  triggerSource: widget
			origin: REMOTE
			time_fired: "2022-08-18T02:53:27.496624+00:00"
			context:
			  id: 01GAQDXDJ8XVAQ2G874PX38Z8K
			  parent_id: null
			  user_id: f7475969d50d4172ae9e5c0e7a954097
		 */

		[JsonPropertyName("ActionID")] // Default is using snake_casing
		public Guid ActionId { get; set; }

		[JsonPropertyName("ActionName")] // Default is using snake_casing
		public string ActionName { get; set; }

		[JsonPropertyName("sourceDevicePermanentID")]
		public Guid SourceDeviceUuid { get; set; }

		[JsonPropertyName("SourceDeviceID")] // Default is using snake_casing
		public string SourceDeviceId { get; set; }

		[JsonPropertyName("SourceDeviceName")] // Default is using snake_casing
		public string SourceDeviceName { get; set; }

		[JsonPropertyName("TriggerSource")] // Default is using snake_casing
		public string TriggerSource { get; set; }

	}
}