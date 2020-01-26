using System;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api
{
	public class UnsubscribeCommand : HomeAssistantCommand
	{
		public UnsubscribeCommand(int subscriptionId)
		{
			Subscription = subscriptionId;
		}

		public string Type => "unsubscribe_events";

		public int Subscription { get; }
	}
}