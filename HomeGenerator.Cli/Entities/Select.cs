using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Select<TValue>(TValue Value);

public record InputSelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : SelectEntity<TValue>(Id, Hub)
	where TValue : struct, Enum;

public record SelectEntity<TValue>(EntityId Id, IHomeAssistantHub Hub) : Entity<Select<TValue>>(Id, Hub)
	where TValue : struct, Enum
{
	/// <inheritdoc />
	protected override Select<TValue> Parse(EntityState raw)
		=> new(raw.GetState<TValue>(Id));
}