using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands;

public struct SetPosition : ICommand
{
	/// <summary>
	/// The position between 0 and 1
	/// </summary>
	public double Position { get; }

	public SetPosition(double position)
	{
		Position = position;
	}
}