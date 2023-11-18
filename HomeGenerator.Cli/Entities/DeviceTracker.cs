using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

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