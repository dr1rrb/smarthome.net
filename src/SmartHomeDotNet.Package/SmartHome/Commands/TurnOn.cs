using System;
using System.Drawing;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// A command to turn on a device
	/// </summary>
	public struct TurnOn : ICommand
	{
		public TurnOn(double brightness)
		{
			Level = brightness;
			Color = default;
			Duration = default;
		}

		public TurnOn(Color color)
		{
			Level = default;
			Color = color;
			Duration = default;
		}

		public TurnOn(TimeSpan transition)
		{
			Level = default;
			Color = default;
			Duration = transition;
		}

		public TurnOn(double brightness, Color color)
		{
			Level = brightness;
			Color = color;
			Duration = default;
		}

		public TurnOn(double brightness, TimeSpan transition)
		{
			Level = brightness;
			Color = default;
			Duration = transition;
		}

		public TurnOn(double brightness, Color color, TimeSpan transition)
		{
			Level = brightness;
			Color = color;
			Duration = transition;
		}

		/// <summary>
		/// The optional target color for RGB light
		/// </summary>
		public Color? Color { get; set; }

		/// <summary>
		/// The optional target level for dimmable devices
		/// </summary>
		public double? Level { get; set; }

		/// <summary>
		/// The optional duration of the fade in for dimmable devices
		/// </summary>
		public TimeSpan? Duration { get; set; }
	}
}