using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A device for the sun component <see cref="https://www.home-assistant.io/components/sun/"/>
	/// </summary>
	public class Sun : Device
	{
		/// <summary>
		/// States of the sun
		/// </summary>
		public enum States
		{
			/// <summary>
			/// Sun is below the horizon
			/// </summary>
			BelowHorizon,

			/// <summary>
			/// Sun is above the horizon
			/// </summary>
			AboveHorizon
		}

		/// <summary>
		/// Gets the current state of the sun
		/// </summary>
		public States State
		{
			get
			{
				switch (Raw.state)
				{
					case "above_horizon": return States.AboveHorizon;
					case "below_horizon": return States.BelowHorizon;
					default: throw new ArgumentOutOfRangeException("State", "Unknown state " + Raw.state);
				}
			}
		}

		public DateTimeOffset NextDawn => DateTimeOffset.Parse(Raw.next_dawn?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public DateTimeOffset NextDusk => DateTimeOffset.Parse(Raw.next_dusk?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public DateTimeOffset NextMidnight => DateTimeOffset.Parse(Raw.next_midnight?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public DateTimeOffset NextNoon => DateTimeOffset.Parse(Raw.next_noon?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public DateTimeOffset NextRising => DateTimeOffset.Parse(Raw.next_rising?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public DateTimeOffset NextSetting => DateTimeOffset.Parse(Raw.next_setting?.ToString().Trim('\"'), CultureInfo.InvariantCulture);

		public double Elevation => double.Parse(Raw.elevation, CultureInfo.InvariantCulture);

		public double Azimuth => double.Parse(Raw.azimuth, CultureInfo.InvariantCulture);
	}
}
