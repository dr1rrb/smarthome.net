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
		/// <summary>
		/// The optional target color for light
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