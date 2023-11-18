using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct InputText(string Value);

public sealed record InputTextEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<InputText>(Id, Hub)
{
	/// <inheritdoc />
	protected override InputText Parse(EntityState raw)
		=> new(raw.State);
}