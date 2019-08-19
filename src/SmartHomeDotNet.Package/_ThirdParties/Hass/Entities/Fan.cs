using System;
using System.Drawing;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="Fan"/> device which allows type inference
	/// </summary>
	public interface IFan : IDevice<IFan>, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
	{
	}

	/// <summary>
	/// A device which can interact with the Fan component: <seealso cref="https://www.home-assistant.io/components/fan/"/>
	/// </summary>
	public class Fan : Device, IFan
	{
		/// <summary>
		/// Enumeration fo the possible fan speed
		/// </summary>
		public enum Speeds
		{
			/// <summary>
			/// Fan is off
			/// </summary>
			/// <remarks>This value is always present in <see cref="Fan.SupportedSpeeds"/>.</remarks>
			Off = 0,
			
			/// <summary>
			/// Low speed
			/// </summary>
			Low = 1,

			/// <summary>
			/// Medium speed
			/// </summary>
			Medium = 2,

			/// <summary>
			/// High speed
			/// </summary>
			High = 3
		}

		/// <summary>
		/// Gets a boolean which indicates if the fan is currently on or not
		/// </summary>
		public bool IsOn => Raw.state == "on";

		/// <summary>
		/// Gets the currently configured speed of the fan
		/// </summary>
		/// <remarks>
		/// This *might* be the speed the last time that the fan was on.
		/// So always prefer to check <see cref="IsOn"/> to determine is the fan is off or not.
		/// </remarks>
		public Speeds Speed => ParseSpeed(Raw.speed);

		/// <summary>
		/// Gets the speed that are supported by this fan
		/// </summary>
		public Speeds[] SupportedSpeeds => ((string) Raw.speed_list)
			.Split(',')
			.Select(ParseSpeed)
			.Concat(new[] {Speeds.Off}) // 'Off' is always supported, but not necessarily reported
			.Distinct()
			.ToArray();

		private static Speeds ParseSpeed(string value)
			=> (Speeds) Enum.Parse(typeof(Speeds), value.Trim('[', ']', '"', ' '), ignoreCase: true);
	}
}