﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.Hass.Events;

namespace SmartHomeDotNet.Hass
{
	public static class HomeAssistantWebSocketApiExtensions
	{
		/// <summary>
		/// Gets and observable sequence of the "ios.action_fired" event.
		/// </summary>
		/// <param name="wsApi">The Home-Assistant hub.</param>
		/// <returns>An observable sequence which produces a values each time the event ios.action_fired is raised by Home-Assistant.</returns>
		public static IObservable<Timestamped<IosAction>> ObserveIosActions(this HomeAssistantWebSocketApi wsApi)
			=> wsApi.Observe("ios.action_fired").Select(evt => new Timestamped<IosAction>(evt.GetData<IosAction>(), evt.Time));
	}
}