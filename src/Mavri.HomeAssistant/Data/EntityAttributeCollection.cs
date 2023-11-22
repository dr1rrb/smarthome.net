using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Mavri.HomeAssistant.Utils;

namespace Mavri.Ha.Data;

[JsonConverter(typeof(EntityAttributeCollectionJsonConverter))]
public record EntityAttributeCollection
{
	private readonly JsonObject _node;
	private readonly JsonSerializerOptions _options;

	public EntityAttributeCollection(JsonObject node, JsonSerializerOptions options)
	{
		_node = node;
		_options = options;
	}

	public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
	{
		if (!_node.TryGetPropertyValue(key, out var valueNode) || valueNode is null)
		{
			value = default;
			return false;
		}

		if (valueNode is JsonValue va)
		{
			if (typeof(T).IsAssignableTo(typeof(Enum)) && va.TryGetValue(out string? rawValue))
			{
				if (Enum.TryParse(typeof(T), NamingStrategy.ToCSharpCamel(rawValue), ignoreCase: true, out var enumValue))
				{
					value = (T)enumValue;
					return true;
				}
				else
				{
					value = default;
					return false;
				}
			}
			else
			{
				return va.TryGetValue(out value);
			}
		}
		else
		{
			value = valueNode.Deserialize<T>(_options);
			return value is not null;
		}
	}

	public bool IsSet(string key) 
		=> _node.ContainsKey(key);

	private void Set<T>(string key, T value)
	{
		throw new NotImplementedException();
	}
}