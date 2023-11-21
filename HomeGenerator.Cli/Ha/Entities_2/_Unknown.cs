using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha.Entities;

public sealed record UnknownEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<EntityState>(Id, Hub)
{
	/// <inheritdoc />
	protected override EntityState Parse(EntityState raw)
		=> raw;
}