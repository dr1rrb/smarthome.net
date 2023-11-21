using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands;

public struct Open : ICommand
{
	/// <summary>
	/// The expected duration needed to open the cover
	/// </summary>
	public TimeSpan? Duration { get; }

	public Open(TimeSpan? duration = null)
	{
		Duration = duration;
	}
}