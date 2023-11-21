using SmartHomeDotNet.Hass;
using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.SmartHome.Commands;

namespace Mavri.Ha.Entities;

public record struct Select<TValue>(TValue Value);

public record InputSelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : SelectEntity<TValue>(Id, Hub)
	where TValue : struct, Enum;

public record SelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : Entity<Select<TValue>>(Id, Hub), ISupport<SmartHomeDotNet.Hass.Commands.Select<TValue>>
	where TValue : struct, Enum
{
	/// <inheritdoc />
	protected override Select<TValue> Parse(EntityState raw)
		=> new(raw.GetState<TValue>(Id));
}