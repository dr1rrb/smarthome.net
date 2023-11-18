using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

public sealed record SwitchEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<bool>(Id, Hub)
{
	/// <inheritdoc />
	protected override bool Parse(EntityState raw)
		=> raw.GetOnOffState(Id);
}