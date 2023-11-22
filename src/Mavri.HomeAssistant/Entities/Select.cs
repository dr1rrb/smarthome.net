using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Select<TValue>(TValue Value);

public record InputSelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : SelectEntity<TValue>(Id, Hub)
	where TValue : struct, Enum;

public record SelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : Entity<Select<TValue>>(Id, Hub), ISupport<Set<TValue>>
	where TValue : struct, Enum
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override Select<TValue> Parse(EntityState raw)
		=> new(raw.GetState<TValue>(Id));
}