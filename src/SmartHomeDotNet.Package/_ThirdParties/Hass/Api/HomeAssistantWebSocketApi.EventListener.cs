using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Api
{
	partial class HomeAssistantWebSocketApi
	{
		private class EventListener : IObservable<HomeAssistantEvent>
		{
			private readonly SerialDisposable _connectionSubscription = new SerialDisposable();
			private readonly ReplaySubject<HomeAssistantEvent> _events;
			private readonly HomeAssistantWebSocketApi _owner;
			private readonly string _type;

			private int _activeSubscriptions;

			public EventListener(HomeAssistantWebSocketApi owner, string type)
			{
				_owner = owner;
				_type = type;
				_events = new ReplaySubject<HomeAssistantEvent>(0, _owner._scheduler);
			}

			public void OnNext(JsonElement evt)
			{
				try
				{
					// That's sad: we gonna have to re-parse it (GetRawText) ... but System.Text.Json does not allow to extract a node (meh! :/),
					// and anyway the .Clone() will do almost do the same as it will copy the source text into a memory.
					// var data = evt.GetProperty("data").Clone();
					var data = evt.GetProperty("data").GetRawText();
					var time = evt.GetProperty("time_fired").GetDateTimeOffset();

					_events.OnNext(new HomeAssistantEvent(_type, time, data));
				}
				catch (Exception e)
				{
					this.Log().Error($"Failed to dispatch event '{_type}'", e);
				}
			}

			private IDisposable SubscribeToEvent()
			{
				IDisposable activeConnectionChangedSubscription;
				if (Interlocked.Increment(ref _activeSubscriptions) == 1)
				{
					var connectionsChangedSubscription = new CompositeDisposable(2);
					var currentConnectionEventSubscription = new SerialDisposable();

					lock (_connectionSubscription)
					{
						activeConnectionChangedSubscription = _connectionSubscription.Disposable = connectionsChangedSubscription;
					}

					_owner._getAndObserveConnection.Subscribe(OnConnectionChanged).DisposeWith(connectionsChangedSubscription);
					currentConnectionEventSubscription.DisposeWith(connectionsChangedSubscription);

					void OnConnectionChanged(Connection connection)
						=> currentConnectionEventSubscription.Disposable = connection == null
							? Disposable.Empty
							: _owner._scheduler.ScheduleAsync(connection, SubscribeOnConnection);
				}
				else
				{
					activeConnectionChangedSubscription = _connectionSubscription.Disposable;
				}

				return Disposable.Create(() =>
				{
					if (Interlocked.Decrement(ref _activeSubscriptions) == 0)
					{
						lock (_connectionSubscription)
						{
							// If we concurrently re-subscribed to event, make sure to not abort
							// the new subscription that was just created.
							if (activeConnectionChangedSubscription == _connectionSubscription.Disposable)
							{
								_connectionSubscription.Disposable = Disposable.Empty;
							}
						}
					}
				});

				async Task<IDisposable> SubscribeOnConnection(IScheduler scheduler, Connection connection, CancellationToken ct)
				{
					try
					{
						using (var subscribeCommand = await connection.Send(new SubscribeCommand(_type), ct))
						{
							await subscribeCommand.GetResult();
							return Disposable.Create(() => UnSubscribeFromConnection(connection, subscribeCommand.Id));
						}
					}
					catch (Exception e)
					{
						this.Log().Error("Subscription failed", e);

						return Disposable.Empty;
					}
				}

				async void UnSubscribeFromConnection(Connection connection, int subscriptionId)
				{
					try
					{
						if (connection.IsConnected)
						{
							await connection.Execute(new UnsubscribeCommand(subscriptionId), CancellationToken.None);
						}
					}
					catch (Exception e)
					{
						this.Log().Debug($"Failed to un-subscribe from connection: {e}");
					}
				}
			}

			/// <inheritdoc />
			public IDisposable Subscribe(IObserver<HomeAssistantEvent> observer)
				=> new CompositeDisposable
				{
					_events.Subscribe(observer),
					_owner.EnsureConnected(),
					SubscribeToEvent()
				};
		}
	}
}