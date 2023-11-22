using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public enum PresenceState
{
	Home,
	NotHome,
}

public sealed record DeviceTrackerEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<PresenceState>(Id, Hub)
{
	/// <inheritdoc />
	protected override PresenceState Parse(EntityState raw)
		=> raw.GetState<PresenceState>(Id);
}