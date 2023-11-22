using System;
using System.Drawing;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Light(bool IsOn, double? Level, Color? Color, string? Effect);

public record LightEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Light>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	// Known possible values:
	//		unavailable
	//		unknown
	//		on
	//		off

	/// <inheritdoc />
	protected override Light Parse(EntityState raw)
	{
		var isOn = raw.GetOnOffState(Id);
		var level = raw.Attributes.TryGet("brightness", out double brightness) ? brightness / 255.0 : default(double?);
		var color = raw.Attributes.TryGet("rgb_color", out int[]? channels) && channels is { Length: 3 } ? Color.FromArgb(channels[0], channels[1], channels[2]) : default(Color?);
		raw.Attributes.TryGet("effect", out string? effect);

		return new Light(isOn, level, color, effect);
	}
}