using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands;

public struct Close : ICommand
{
	/// <summary>
	/// The expected duration needed to close the cover
	/// </summary>
	public TimeSpan? Duration { get; }

	public Close(TimeSpan? duration = null)
	{
		Duration = duration;
	}
}