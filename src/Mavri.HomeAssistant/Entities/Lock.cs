using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Data;

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

public sealed record LockEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Lock>(Id, Hub), ISupport<Commands.Lock>, ISupport<Commands.Unlock>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override Lock Parse(EntityState raw)
		=> new(raw.GetState<LockState>(Id));
}