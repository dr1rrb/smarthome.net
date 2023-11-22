using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to decrement teh current value of a thing (e.g. a number)
/// </summary>
public readonly record struct Decrement : ICommand;