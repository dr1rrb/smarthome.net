using System;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Camera(CameraState State, Uri Picture);

public enum CameraState
{
	Idle,
	Recording,
	Streaming
}

public sealed record CameraEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Camera>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override Camera Parse(EntityState raw)
	{
		var state = Enum.Parse<CameraState>(raw.State, ignoreCase: true);
		var picture = raw.Attributes.TryGet("entity_picture", out string? uri) ? uri : throw new InvalidOperationException($"No entity_picture for camera entity '{Id}'.");

		return new Camera(state, new Uri(Hub.BaseUri.OriginalString + picture));
	}
}
