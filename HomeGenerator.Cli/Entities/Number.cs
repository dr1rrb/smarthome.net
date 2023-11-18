using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Globalization;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Number(double Value, double Min, double Max, double Step);

public sealed record InputNumberEntity(EntityId Id, IHomeAssistantHub Hub) : NumberEntity(Id, Hub);

public record NumberEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Number>(Id, Hub)
{
	/// <inheritdoc />
	protected override Number Parse(EntityState raw)
	{
		var value = double.Parse(raw.State, CultureInfo.InvariantCulture);
		var min = raw.Attributes.Get<double>("min", Id);
		var max = raw.Attributes.Get<double>("max", Id);
		var step = raw.Attributes.Get<double>("step", Id);

		return new Number(value, min, max, step);
	}
}