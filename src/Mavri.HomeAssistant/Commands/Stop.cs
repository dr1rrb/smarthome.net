using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to stop a thing (e.g. a Cover)
/// </summary>
public readonly record struct Stop : ICommand;