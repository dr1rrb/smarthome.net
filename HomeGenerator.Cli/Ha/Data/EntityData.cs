using System;
using System.Linq;
using System.Text.Json.Serialization;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha.Data;

/*
	{
		"area_id": null,
		"config_entry_id": "6923d162b036f7e4e8803283be419948",
		"device_id": "f377941666bb27d80ac35ee23f5325db",
		"disabled_by": null,
		"entity_category": "diagnostic",
		"entity_id": "sensor.workbench_light_node_status",
		"has_entity_name": true,
		"hidden_by": null,
		"icon": null,
		"id": "103b30726f96738d52f2396deb2389c8",
		"name": "Lumière établis: Node Status",
		"options": {
			"conversation": {
				"should_expose": false
			}
		},
		"original_name": "Node status",
		"platform": "zwave_js",
		"translation_key": null,
		"unique_id": "4059427701.15.node_status"
	}
*/
internal record EntityData(
	[property: JsonPropertyName("entity_id")] string RawId,
	[property: JsonPropertyName("has_entity_name")] bool HasName,
	string? Name = null,
	string? OriginalName = null,
	string? AreaId = null,
	string? DeviceId = null)
{
	[JsonIgnore]
	public EntityId Id { get; } = EntityId.Parse(RawId);
}