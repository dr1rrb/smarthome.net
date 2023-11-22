using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Mavri.Ha.Data;
using Mavri.Logging;
using Microsoft.Extensions.Logging;

namespace Mavri.Ha.Entities;

public abstract record Entity<T>(EntityId Id, IHomeAssistantHub Hub) : IThing<EntityId, T?>, IEntity
	where T : notnull
{
	private readonly BehaviorSubject<EntityState<T>> _state = new(default!);

	/// <inheritdoc />
	void IEntity.Publish(EntityState raw)
	{
		T? value = default;
		if (raw is { IsUnavailable: false, IsUnknown: false })
		{
			try
			{
				value = Parse(raw);
			}
			catch (Exception error)
			{
				this.Log().LogError($"Failed to parse state of entity {Id}.", error);
			}
		}

		try
		{
			_state.OnNext(new(value, raw));
		}
		catch (Exception error)
		{
			this.Log().LogError($"Failed to published updated state of entity {Id}.", error);
		}
	}

	/// <summary>
	/// Gets an observable sequence of the complete state of that entity.
	/// </summary>
	public IObservable<EntityState<T>> State => _state
		.SkipWhile(static state => state.Raw is not null)
		// Ensure to never go back in time. This is important at startup where we publish the initial states obtain using another API
		.Scan(static (previous, current) => current.Raw.LastUpdated > previous.Raw.LastUpdated ? current : previous)
		.DistinctUntilChanged();

	/// <inheritdoc />
	public IDisposable Subscribe(IObserver<T?> observer)
		=> State.Select(static state => state.Value).DistinctUntilChanged().Subscribe(observer);

	protected abstract T Parse(EntityState raw);
}