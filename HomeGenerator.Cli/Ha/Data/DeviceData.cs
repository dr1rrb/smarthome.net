using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Data;


/*
	{
		"area_id": "950f5d119c0e4967b9ba243dae5c54e3",
		"configuration_url": null,
		"config_entries": ["fb59f1e0999016c68b5786a52ca07fff"],
		"connections": [],
		"disabled_by": null,
		"entry_type": null,
		"hw_version": null,
		"id": "6ca196c5a220b6094704091a490f3b87",
		"identifiers": [["zwave_js", "3277824893-21"], ["zwave_js", "3277824893-21-798:5:1"]],
		"manufacturer": "Inovelli",
		"model": "LZW42",
		"name_by_user": "Globe de la chambre",
		"name": "inovelli_lzw42_001",
		"serial_number": null,
		"sw_version": "2.28",
		"via_device_id": "5b881e3ffafaeb54f06ae10531873786"
	}
*/
internal record DeviceData(
	string Id,
	string Name,
	[property: JsonPropertyName("name_by_user")] string? FriendlyName = null,
	string? AreaId = null,
	string? Model = null,
	string? Manufacturer = null);