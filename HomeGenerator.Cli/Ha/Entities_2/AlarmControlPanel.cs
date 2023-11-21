using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha.Entities;

public record struct AlarmControlPanel(AlarmControlPanelState State);

public enum AlarmControlPanelState
{
	Disarmed,
	ArmedHome,
	ArmedAway,
	ArmedNight,
	ArmedVacation,
	ArmedCustomBypass,
	Pending,
	Arming,
	Disarming,
	Triggered,
}

public sealed record AlarmControlPanelEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<AlarmControlPanel>(Id, Hub)
{
	/// <inheritdoc />
	protected override AlarmControlPanel Parse(EntityState raw)
	{
		var state = raw.GetState<AlarmControlPanelState>(Id);

		return new AlarmControlPanel(state);
	}
}