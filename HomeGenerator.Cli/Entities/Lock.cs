using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Lock(LockState State);

public enum LockState
{
	Locked,
	Unlocked,
	Locking,
	Unlocking,
	Jammed
}

public sealed record LockEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Lock>(Id, Hub)
{
	/// <inheritdoc />
	protected override Lock Parse(EntityState raw)
		=> new(raw.GetState<LockState>(Id));
}