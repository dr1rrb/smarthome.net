using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A device for the Person integration: <seealso cref="https://www.home-assistant.io/integrations/person/"/>
	/// </summary>
	public class Person : Device
	{
		/// <summary>
		/// The name of this person
		/// </summary>
		public string Name
			=> TryGetValue("friendly_name", out var name) && name.HasValue()
				? (Newtonsoft.Json.JsonConvert.DeserializeObject(name) as string ?? string.Empty)
				: string.Empty;

		/// <summary>
		/// The current state of this person
		/// </summary>
		public PresenceState State
		{
			get
			{
				switch ((Raw.state as string)?.ToLowerInvariant() ?? string.Empty)
				{
					case "home": return PresenceState.Present;
					case "not_home": return PresenceState.Away;
					default: return PresenceState.Unknown;
				}
			}
		}
	}
}
