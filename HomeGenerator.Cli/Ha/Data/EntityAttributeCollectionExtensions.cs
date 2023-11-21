using System;
using System.Collections.Immutable;
using System.Linq;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha.Data;

public static class EntityAttributeCollectionExtensions
{
	public static T Get<T>(this EntityAttributeCollection attributes, string key, EntityId id)
		=> attributes.TryGet(key, out T? value) ? value : throw new InvalidOperationException($"Attribute {key} is missing for entity '{id}'.");

	public static T GetOrDefault<T>(this EntityAttributeCollection attributes, string key, T defaultValue)
		=> attributes.TryGet(key, out T? value) ? value : defaultValue;

	public static ImmutableArray<T> GetArray<T>(this EntityAttributeCollection attributes, string key, EntityId id)
		=> attributes.TryGet(key, out ImmutableArray<T> value) ? value : throw new InvalidOperationException($"Attribute {key} is missing for entity '{id}'.");

	public static ImmutableArray<T> GetArrayOrDefault<T>(this EntityAttributeCollection attributes, string key)
		=> attributes.TryGet(key, out ImmutableArray<T> value) ? value : ImmutableArray<T>.Empty;
}