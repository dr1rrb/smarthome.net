using SmartHomeDotNet.Hass;
using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.SmartHome.Commands;

namespace Mavri.Ha.Entities;

public record struct Lock(LockState State);

public enum LockState
{
	Locked,
	Unlocked,
	Locking,
	Unlocking,
	Jammed
}

public sealed record LockEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Lock>(Id, Hub), ISupport<SmartHomeDotNet.Hass.Commands.Lock>, ISupport<SmartHomeDotNet.Hass.Commands.Unlock>
{
	/// <inheritdoc />
	protected override Lock Parse(EntityState raw)
		=> new(raw.GetState<LockState>(Id));
}