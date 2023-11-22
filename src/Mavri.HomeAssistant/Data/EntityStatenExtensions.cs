using System;
using System.Linq;
using Mavri.HomeAssistant.Utils;

namespace Mavri.Ha.Data;

public static class EntityStatenExtensions
{
	public static T GetState<T>(this EntityState state, EntityId id)
		where T : struct, Enum
		=> Enum.TryParse(NamingStrategy.ToCSharpCamel(state.State), ignoreCase: true, out T value) ? value : throw new InvalidOperationException($"Unknown state '{state.State}' for entity '{id}'.");

	public static bool GetOnOffState(this EntityState state, EntityId id)
		=> state.State.ToLowerInvariant() switch
		{
			"on" => true,
			"off" => false,
			_ => throw new InvalidOperationException($"Unknown state '{state.State}' for a entity '{id}'.")
		};
}