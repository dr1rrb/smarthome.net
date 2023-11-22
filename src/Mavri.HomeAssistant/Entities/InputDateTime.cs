using System;
using System.Globalization;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct InputDateTime(DateTime Value, bool HasDate, bool HasTime);

public sealed record InputDateTimeEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<InputDateTime>(Id, Hub)
{
	/// <inheritdoc />
	protected override InputDateTime Parse(EntityState raw)
	{
		var state = DateTime.Parse(raw.State, CultureInfo.InvariantCulture);
		var hasDate = raw.Attributes.TryGet("has_date", out bool d) ? d : throw new InvalidOperationException($"No has_date for input_datetime entity '{Id}'.");
		var hasTime = raw.Attributes.TryGet("has_time", out bool t) ? t : throw new InvalidOperationException($"No has_time for input_datetime entity '{Id}'.");

		return new InputDateTime(state, hasDate, hasTime);
	}
}
