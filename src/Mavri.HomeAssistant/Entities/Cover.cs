using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Cover(CoverState State, double? Position);

public enum CoverState
{
	Open,
	Closed,
	Opening,
	Closing,
}

public sealed record CoverEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Cover>(Id, Hub), ISupport<Toggle>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override Cover Parse(EntityState raw)
	{
		var state = Enum.Parse<CoverState>(raw.State, ignoreCase: true);
		var position = raw.Attributes.TryGet("current_position", out double current) ? current / 100.0 : default(double?);

		return new(state, position);
	}
}