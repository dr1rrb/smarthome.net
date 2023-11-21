using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha.Entities;

public record BinarySensorEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<bool>(Id, Hub)
{
	// Known possible values:
	//		unavailable
	//		unknown
	//		on
	//		off

	/// <inheritdoc />
	protected override bool Parse(EntityState raw)
		=> raw.GetOnOffState(Id);
}