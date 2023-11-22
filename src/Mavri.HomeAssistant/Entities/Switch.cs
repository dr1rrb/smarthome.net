using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public sealed record SwitchEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<bool>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override bool Parse(EntityState raw)
		=> raw.GetOnOffState(Id);
}