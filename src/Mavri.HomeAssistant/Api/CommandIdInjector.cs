using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mavri.Ha.Api;

internal class CommandIdInjector : JsonConverter<int>
{
	/// <inheritdoc />
	public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.Skip();
		return -1;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
		=> writer.WriteNumberValue(CommandId.Current);
}