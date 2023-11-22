using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Siren(bool IsOn);

public sealed record SirenEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Siren>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;


	/// <inheritdoc />
	protected override Siren Parse(EntityState raw)
		=> new(raw.GetOnOffState(Id));
}