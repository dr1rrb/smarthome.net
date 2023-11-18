using System;
using System.Linq;
using System.Text.Json;

namespace HomeGenerator.Cli.Utils;

internal static class SourceJson
{
	/// <summary>
	/// Gets the JSON document used to create the given object.
	/// </summary>
	/// <param name="value">The instance for which to retrieve the source json.</param>
	/// <returns>The source json document if any.</returns>
	public static JsonDocument? Get<T>(T value)
		where T : class
		=> SourceJson<T>.Get(value);

	public static string? ToString<T>(T value)
		where T : class
		=> Get(value)?.Serialize(new JsonWriterOptions { Indented = true, SkipValidation = true });
}