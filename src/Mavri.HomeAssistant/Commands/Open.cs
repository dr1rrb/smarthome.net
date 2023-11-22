using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to open a thing (e.g. a cover)
/// </summary>
/// <param name="Duration">The expected duration needed to open the cover</param>
public readonly record struct Open(TimeSpan? Duration = null) : ICommand;