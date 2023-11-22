using System;
using System.Globalization;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record ButtonEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<DateTimeOffset>(Id, Hub), ISupport<Press>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	// Known possible values:
	//		unavailable
	//		unknown
	//		2023-10-17T02:12:53.408671+00:00

	/// <inheritdoc />
	protected override DateTimeOffset Parse(EntityState raw)
		=> DateTimeOffset.Parse(raw.State, CultureInfo.InvariantCulture);
}