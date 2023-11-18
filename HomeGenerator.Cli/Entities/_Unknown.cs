using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

public sealed record UnknownEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<EntityState>(Id, Hub)
{
	/// <inheritdoc />
	protected override EntityState Parse(EntityState raw)
		=> raw;
}