using System;
using System.Collections.Immutable;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

public record struct Person(PresenceState State, IImmutableList<EntityId> Trackers);

public sealed record PersonEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Person>(Id, Hub)
{
	/// <inheritdoc />
	protected override Person Parse(EntityState raw)
	{
		var state = raw.GetState<PresenceState>(Id);
		var trackers = raw.Attributes.GetArrayOrDefault<string>("device_trackers");

		return new Person(state, trackers.Select(id => (EntityId)id).ToImmutableArray());
	}
}