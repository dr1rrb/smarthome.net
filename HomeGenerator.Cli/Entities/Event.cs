using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Globalization;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Event<TEvent>(DateTimeOffset Timestamp, TEvent Value);

public sealed record EventEntity<TEvent>(EntityId Id, IHomeAssistantHub Hub) : Entity<Event<TEvent>>(Id, Hub)
	where TEvent : struct, Enum
{
	/// <inheritdoc />
	protected override Event<TEvent> Parse(EntityState raw)
		=> new(DateTimeOffset.Parse(raw.State, CultureInfo.InvariantCulture), raw.Attributes.Get<TEvent>("event_type", Id));
}