using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct InputText(string Value);

public sealed record InputTextEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<InputText>(Id, Hub)
{
	/// <inheritdoc />
	protected override InputText Parse(EntityState raw)
		=> new(raw.State);
}