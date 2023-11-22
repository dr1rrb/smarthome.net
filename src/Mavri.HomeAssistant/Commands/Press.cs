using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to press a thing (e.g. a button)
/// </summary>
public readonly record struct Press : ICommand;