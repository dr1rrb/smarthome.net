using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.IKEA.Devices
{
	/// <summary>
	/// The remote toggle switch from IKEA
	/// </summary>
	public class SquareToggleButton : Device
	{
		/// <summary>
		/// Gets the action applied on the button
		/// </summary>
		public Actions Action { get; private set; }

		/// <summary>
		/// Indicates if the <see cref="Action"/> is meaning full or not. (cf. <see cref="Actions.Unknown"/>)
		/// </summary>
		public bool IsValidAction => Action != Actions.Unknown;

		/// <inheritdoc />
		protected override void OnInit()
		{
			if (TryGetValue("click", out var action) // Z2M
				|| TryGetValue("state", out action)) // HA
			{
				switch (action.ToLowerInvariant())
				{
					case "on":
						Action = Actions.On;
						break;
					case "off":
						Action = Actions.Off;
						break;
					case "brightness_up":
						Action = Actions.BrightnessUp;
						break;
					case "brightness_down":
						Action = Actions.BrightnessDown;
						break;
					case "brightness_stop":
						Action = Actions.BrightnessStop;
						break;
				}
			}
		}

		public enum Actions
		{
			/// <summary>
			/// The action is unknown. This is usually an action published after a valid action
			/// in order to clean the state. Those values should be ignored (cf. <see cref="SquareToggleButton.IsValidAction"/>).
			/// </summary>
			Unknown,

			/// <summary>
			/// Short press on the "on"
			/// </summary>
			On,

			/// <summary>
			/// Short press on the "off"
			/// </summary>
			Off,

			/// <summary>
			/// Starts long press on the "on"
			/// </summary>
			BrightnessUp,

			/// <summary>
			/// Starts long press on the "off"
			/// </summary>
			BrightnessDown,

			/// <summary>
			/// End of long press ("on" or "off")
			/// </summary>
			BrightnessStop
		}
	}
}
