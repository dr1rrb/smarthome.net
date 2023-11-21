using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Siren(bool IsOn);

public sealed record SirenEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Siren>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
{
	/// <inheritdoc />
	protected override Siren Parse(EntityState raw)
		=> new(raw.GetOnOffState(Id));
}