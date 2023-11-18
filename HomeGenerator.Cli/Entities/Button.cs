using System;
using System.Globalization;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace HomeGenerator.Cli;

[ComponentEntity("button")]
public record ButtonEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<DateTimeOffset>(Id, Hub)
{
	// Known possible values:
	//		unavailable
	//		unknown
	//		2023-10-17T02:12:53.408671+00:00

	/// <inheritdoc />
	protected override DateTimeOffset Parse(EntityState raw)
		=> DateTimeOffset.Parse(raw.State, CultureInfo.InvariantCulture);
}