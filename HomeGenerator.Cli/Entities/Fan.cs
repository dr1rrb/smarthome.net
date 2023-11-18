using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Fan(bool IsOn, double? Speed, double? SpeedStep);

public sealed record FanEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Fan>(Id, Hub)
{
	/// <inheritdoc />
	protected override Fan Parse(EntityState raw)
	{
		var isOn = raw.GetOnOffState(Id);
		var speed = raw.Attributes.TryGet("percentage", out double percentage) ? percentage / 100.0 : default(double?);
		var speedStep = raw.Attributes.TryGet("percentage_step", out double percentageStep) ? percentageStep / 100.0 : default(double?);

		return new Fan(isOn, speed, speedStep);
	}
}