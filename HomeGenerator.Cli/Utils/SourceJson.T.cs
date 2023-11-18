using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeGenerator.Cli.Utils;

internal class SourceJson<T> : JsonConverter<T>
	where T : class
{
	private static readonly ConditionalWeakTable<T, JsonDocument> _documents = new();

	public static SourceJson<T> Instance { get; } = new();

	private SourceJson()
	{
	}

	/// <summary>
	/// Gets the JSON document used to create the given object.
	/// </summary>
	/// <param name="value">The instance for which to retrieve the source json.</param>
	/// <returns>The source json document if any.</returns>
	public static JsonDocument? Get(T value)
		=> _documents.TryGetValue(value, out var json) ? json : default;

	/// <inheritdoc />
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		options = new JsonSerializerOptions(options);
		options.Converters.Remove(this);

		var json = JsonDocument.ParseValue(ref reader);
		var value = json.Deserialize<T>(options);
		if (value is not null)
		{
			_documents.Add(value, json);
		}

		return value;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		=> throw new NotSupportedException("SourceJson only supports deserialization operations.");
}