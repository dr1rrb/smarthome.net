using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Data;

/*
	{
		"entity_id": "sensor.hydroqc_yesterday_morning_peak_saved_consumtion",
		"state": "unavailable",
		"attributes": {
			"unit_of_measurement": "kWh",
			"device_class": "energy",
			"icon": "mdi:home-lightning-bolt",
			"friendly_name": "Hydro Québec Yesterday morning peak saved consumtion"
		},
		"last_changed": "2023-11-04T00:43:20.152984+00:00",
		"last_updated": "2023-11-04T00:43:20.152984+00:00",
		"context": {
			"id": "01HEBWHJ6R1E0VZVVQCPWPXTBP",
			"parent_id": null,
			"user_id": null
		}
	}
*/
public record EntityState(
	[property: JsonPropertyName("entity_id")] string RawEntityId,
	string State,
	EntityAttributeCollection Attributes,
	DateTimeOffset LastChanged,
	DateTimeOffset LastUpdated)
{
	public const string Unavailable = "unavailable";
	public const string Unknown = "unknown";

	[JsonIgnore]
	public EntityId EntityId { get; } = EntityId.Parse(RawEntityId);

	[JsonIgnore]
	public bool IsUnavailable => State is Unavailable;

	[JsonIgnore]
	public bool IsUnknown => State is Unknown;
}