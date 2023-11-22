using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to lock a thing (e.g. a door lock)
/// </summary>
public readonly record struct Unlock(string? Code = null) : ICommand;