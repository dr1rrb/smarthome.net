﻿using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api
{
	public class SubscribeCommand : HomeAssistantCommand
	{
		public SubscribeCommand(string eventType)
		{
			EventType = eventType;
		}

		public string Type => "subscribe_events";

		public string EventType { get; }
	}
}