using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;
using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct Camera(CameraState State, Uri Picture);

public enum CameraState
{
	Idle,
	Recording,
	Streaming
}

public sealed record CameraEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Camera>(Id, Hub)
{
	/// <inheritdoc />
	protected override Camera Parse(EntityState raw)
	{
		var state = Enum.Parse<CameraState>(raw.State, ignoreCase: true);
		var picture = raw.Attributes.TryGet("entity_picture", out string? uri) ? uri : throw new InvalidOperationException($"No entity_picture for camera entity '{Id}'.");

		return new Camera(state, new Uri(Hub.BaseUri.OriginalString + picture));
	}
}
