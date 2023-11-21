using System;
using System.Collections.Immutable;
using System.Linq;
using HomeGenerator.Cli;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.Logging;

namespace Mavri.Ha.Core;

internal sealed class EntitiesUpdater : IDisposable
{
	private readonly IEntityRegistry _entities;
	private readonly HomeAssistantWebSocketApi _api;
	private readonly IDisposable _subscription;
	private readonly CancellationTokenSource _ct = new();

	public EntitiesUpdater(IEntityRegistry entities, HomeAssistantWebSocketApi api)
	{
		_entities = entities;
		_api = api;

		_subscription = api.ObserveEntityState().Subscribe(update => Publish(update.NewState));
		_ = LoadInitialStates(_ct.Token);
	}

	private async Task LoadInitialStates(CancellationToken ct)
	{
		try
		{
			var states = await _api.Send<ImmutableArray<EntityState>>(new GetStatesCommand(), ct);
			foreach (var state in states)
			{
				Publish(state);
			}
		}
		catch (Exception)
		{
			this.Log().Error("Failed to load initial states.");
		}
	}

	private void Publish(EntityState state)
	{
		try
		{
			_entities.Get(state.EntityId)?.Publish(state);
		}
		catch (Exception)
		{
			this.Log().Error($"Failed to publish new state for entity '{state.EntityId}'.");
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_subscription.Dispose();
		_ct.Cancel();
	}
}