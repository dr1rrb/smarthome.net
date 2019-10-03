using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Xiaomi.Devices
{
	public class Button : Device
	{
		public enum Actions
		{
			/// <summary>
			/// Unknown action
			/// </summary>
			Unknown = 0,

			/// <summary>
			/// The button was pressed once
			/// </summary>
			Single,

			/// <summary>
			/// The button was pressed twice
			/// </summary>
			Double
		}

		public Actions Action => 
			(TryGetValue("state", out var raw) // Home assistant
			|| TryGetValue("click", out raw)) // Zigbee2mqtt
			&& Enum.TryParse<Actions>(raw, true, out var action)
				? action
				: Actions.Unknown;
	}
}