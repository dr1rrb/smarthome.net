using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Siren(bool IsOn);

public sealed record SirenEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Siren>(Id, Hub)
{
	/// <inheritdoc />
	protected override Siren Parse(EntityState raw)
		=> new(raw.GetOnOffState(Id));
}