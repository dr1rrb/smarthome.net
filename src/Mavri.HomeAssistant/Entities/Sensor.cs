using System;
using System.Globalization;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

/// <summary>
/// A generic sensor entity for unknown data types
/// </summary>
public abstract record SensorEntity<T>(EntityId Id, IHomeAssistantHub Hub) : Entity<T>(Id, Hub)
	where T : notnull;

public sealed record StringEntity(EntityId Id, IHomeAssistantHub Hub) : SensorEntity<string>(Id, Hub)
{
	/// <inheritdoc />
	protected override string Parse(EntityState raw)
		=> raw.State;
}

public sealed record DoubleEntity(EntityId Id, IHomeAssistantHub Hub) : SensorEntity<double>(Id, Hub)
{
	/// <inheritdoc />
	protected override double Parse(EntityState raw)
		=> double.Parse(raw.State, CultureInfo.InvariantCulture);
}

public sealed record TimestampEntity(EntityId Id, IHomeAssistantHub Hub) : SensorEntity<DateTimeOffset>(Id, Hub)
{
	/// <inheritdoc />
	protected override DateTimeOffset Parse(EntityState raw)
		=> DateTimeOffset.Parse(raw.State, CultureInfo.InvariantCulture);
}

public sealed record EnumEntity<T>(EntityId Id, IHomeAssistantHub Hub) : SensorEntity<T>(Id, Hub)
	where T : struct, Enum
{
	/// <inheritdoc />
	protected override T Parse(EntityState raw)
		=> raw.GetState<T>(Id);
}