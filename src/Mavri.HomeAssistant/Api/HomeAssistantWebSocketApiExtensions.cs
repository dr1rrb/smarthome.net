using System;
using System.Linq;
using System.Reactive.Linq;
using Mavri.Ha.Api;
using Mavri.Ha.Data;

namespace Mavri.Ha.Api;

/// <summary>
/// Extensions over <see cref="HomeAssistantWebSocketApi"/>.
/// </summary>
public static class HomeAssistantWebSocketApiExtensions
{
	/// <summary>
	/// Gets and observable sequence of the "ios.action_fired" event.
	/// </summary>
	/// <param name="wsApi">The Home-Assistant hub.</param>
	/// <returns>An observable sequence which produces a values each time the event ios.action_fired is raised by Home-Assistant.</returns>
	public static IObservable<EntityStateUpdate> ObserveEntityState(this HomeAssistantWebSocketApi wsApi)
		=> wsApi.Observe("state_changed").Select(evt => evt.GetData<EntityStateUpdate>()!);
}