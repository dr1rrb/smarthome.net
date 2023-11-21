using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Data;

public class EntityAttributeCollectionJsonConverter : JsonConverter<EntityAttributeCollection>
{
	/// <inheritdoc />
	public override EntityAttributeCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return JsonNode.Parse(ref reader, new JsonNodeOptions { PropertyNameCaseInsensitive = true }) is JsonObject attributes 
			? new EntityAttributeCollection(attributes, options)
			: throw new JsonException("Cannot read an EntityAttributeCollection");
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, EntityAttributeCollection value, JsonSerializerOptions options)
		=> throw new NotImplementedException();
}