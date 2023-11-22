using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to set the relative position of a thing (e.g. a cover)
/// </summary>
/// <param name="Position">The position between 0 and 1</param>
public readonly record struct SetPosition(double Position) : ICommand;