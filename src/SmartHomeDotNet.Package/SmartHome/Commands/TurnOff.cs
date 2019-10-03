using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// A command to turn off a device
	/// </summary>
	public struct TurnOff : ICommand
	{
		public TurnOff(TimeSpan transition)
		{
			Duration = transition;
		}


		/// <summary>
		/// The optional duration of the fade out for dimmable devices
		/// </summary>
		public TimeSpan? Duration { get; set; }
	}
}