using System;
using System.Globalization;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

/// <summary>
/// A generic sensor entity for unknown data types
/// </summary>
public sealed record SensorEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<string>(Id, Hub)
{
	/// <inheritdoc />
	protected override string Parse(EntityState raw)
		=> raw.State;
}

public sealed record DoubleEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<double>(Id, Hub)
{
	/// <inheritdoc />
	protected override double Parse(EntityState raw)
		=> double.Parse(raw.State, CultureInfo.InvariantCulture);
}

public sealed record TimestampEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<DateTimeOffset>(Id, Hub)
{
	/// <inheritdoc />
	protected override DateTimeOffset Parse(EntityState raw)
		=> DateTimeOffset.Parse(raw.State, CultureInfo.InvariantCulture);
}

public sealed record EnumEntity<T>(EntityId Id, IHomeAssistantHub Hub) : Entity<T>(Id, Hub)
	where T : struct, Enum
{
	/// <inheritdoc />
	protected override T Parse(EntityState raw)
		=> raw.GetState<T>(Id);
}