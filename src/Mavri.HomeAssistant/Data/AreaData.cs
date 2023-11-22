using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Data;

/*
	{
		"aliases": [],
		"area_id": "950f5d119c0e4967b9ba243dae5c54e3",
		"name": "Chambre",
		"picture": null
	}
*/
public record AreaData(
	[property: JsonPropertyName("area_id")] string Id,
	string? Name = null);