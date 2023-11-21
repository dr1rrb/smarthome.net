using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Data;

internal record ConfigData(
	[property: JsonPropertyName("location_name")] string? Name);