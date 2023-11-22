using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to toggle a thing (e.g. a switch, a light)
/// </summary>
/// <param name="Duration">The optional duration of the fade out for dimmable devices</param>
public readonly record struct Toggle(TimeSpan? Duration = null) : ICommand;