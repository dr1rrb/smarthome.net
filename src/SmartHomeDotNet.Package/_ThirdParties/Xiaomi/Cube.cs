using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Xiaomi.Devices
{
	public class Cube : Device
	{
		public enum Actions
		{
			Unknown = 0,

			/// <summary>
			/// The cube was idle, but is being moved, so it's waking up.
			/// </summary>
			WakeUp,

			RotateRight,
			RotateLeft,
			Flip90,
			Flip180,
			Slide,
			Tap,
			Shake
		}

		/// <summary>
		/// Determines if action can be interpreted or not (i.e. all actions except <see cref="Actions.WakeUp"/> and <see cref="Actions.Unknown"/>)
		/// </summary>
		public bool IsValidAction => Action != Actions.WakeUp && Action != Actions.Unknown;

		/// <summary>
		/// Gets the movement that was done on the cube 
		/// </summary>
		public Actions Action { get; private set; }

		/// <summary>
		/// The angle of rotation if <see cref="Action"/> is
		/// <see cref="Actions.RotateLeft"/> or <see cref="Actions.RotateRight"/>.
		/// </summary>
		public double Angle { get; private set; }

		/// <summary>
		/// The current top side of the cube if <see cref="Action"/> is
		/// <see cref="Actions.Flip90"/>, <see cref="Actions.Flip180"/>,
		/// <see cref="Actions.Slide"/> or <see cref="Actions.Tap"/>.
		/// </summary>
		public int Side { get; private set; }

		/// <summary>
		/// The previous side if <see cref="Action"/> is
		/// <see cref="Actions.Flip90"/> or <see cref="Actions.Flip180"/>.
		/// </summary>
		public int PreviousSide { get; private set; }

		/// <inheritdoc />
		protected override void OnInit()
		{
			base.OnInit();

			if (TryGetValue("action", out var action) // Zigbee2MQTT
				|| TryGetValue("state", out action)) // HA
			{
				switch (action.Trim('"'))
				{
					case "rotate_left":
						Action = Actions.RotateLeft;
						Angle = double.Parse(Raw.angle, CultureInfo.InvariantCulture);
						break;

					case "rotate_right":
						Action = Actions.RotateRight;
						Angle = double.Parse(Raw.angle, CultureInfo.InvariantCulture);
						break;

					case "flip90":
						Action = Actions.Flip90;
						PreviousSide = int.Parse(Raw.from_side, CultureInfo.InvariantCulture);
						Side = int.Parse(Raw.to_side, CultureInfo.InvariantCulture);
						break;

					case "flip180":
						Action = Actions.Flip180;
						PreviousSide = int.Parse(Raw.from_side, CultureInfo.InvariantCulture);
						Side = int.Parse(Raw.to_side, CultureInfo.InvariantCulture);
						break;

					case "slide":
						Action = Actions.Slide;
						PreviousSide = Side = int.Parse(Raw.side, CultureInfo.InvariantCulture);
						break;

					case "tap":
						Action = Actions.Tap;
						PreviousSide = Side = int.Parse(Raw.side, CultureInfo.InvariantCulture);
						break;

					case "shake":
						Action = Actions.Shake;
						break;

					case "wakeup":
						Action = Actions.WakeUp;
						break;

					default:
						Action = Actions.Unknown;
						break;
				}
			}
			else
			{
				Action = Actions.Unknown;
			}
		}
	}
}