using System;
using System.Drawing;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to turn on a thing (e.g. a switch, a light)
/// </summary>
/// <param name="Level">The optional target level for dimmable devices</param>
/// <param name="Color">The optional target color for RGB light</param>
/// <param name="Duration">The optional duration of the fade in for dimmable devices</param>
/// <param name="Effect">The optional effect for the smart light devices</param>
public readonly record struct TurnOn(double? Level = null, Color? Color = null, TimeSpan ? Duration = null, string? Effect = null) : ICommand
{
	public TurnOn(double brightness) : this(brightness, null, null, null) { }

	public TurnOn(Color color) : this(null, color, null, null) { }

	public TurnOn(TimeSpan transition) : this(null, null, transition, null) { }

	public TurnOn(string effect) : this(null, null, null, effect) { }

	public TurnOn(double brightness, string effect) : this(brightness, null, null, effect) { }

	public TurnOn(Color color, string effect) : this(null, color, null, effect) { }

	public TurnOn(TimeSpan transition, string effect) : this(null, null, transition, effect) { }

	public TurnOn(double brightness, TimeSpan transition) : this(brightness, null, transition, null) { }

	public TurnOn(double brightness, TimeSpan transition, string effect) : this(brightness, null, transition, effect) { }

	public TurnOn(double brightness, Color color) : this(brightness, color, null, null) { }

	public TurnOn(double brightness, Color color, string effect) : this(brightness, color, null, effect) { }

	public TurnOn(double brightness, Color color, TimeSpan transition) : this(brightness, color, transition, null) { }
}