using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Commands;

namespace Mavri.Ha.Entities;

public sealed record SwitchEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<bool>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
{
	/// <inheritdoc />
	protected override bool Parse(EntityState raw)
		=> raw.GetOnOffState(Id);
}