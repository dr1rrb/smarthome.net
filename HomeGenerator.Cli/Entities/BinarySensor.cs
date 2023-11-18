using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

[ComponentEntity("binary_sensor")]
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