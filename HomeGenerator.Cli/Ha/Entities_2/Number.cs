using SmartHomeDotNet.Hass;
using System;
using System.Globalization;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass.Commands;
using SmartHomeDotNet.SmartHome.Commands;

namespace Mavri.Ha.Entities;

public record struct Number(double Value, double Min, double Max, double Step);

public sealed record InputNumberEntity(EntityId Id, IHomeAssistantHub Hub) : NumberEntity(Id, Hub), ISupport<Increment>, ISupport<Decrement>;

public record NumberEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Number>(Id, Hub), ISupport<SetValue<double>>
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