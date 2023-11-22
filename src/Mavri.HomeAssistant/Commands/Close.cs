using System;
using System.Linq;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to close a thing (e.g. a Cover)
/// </summary>
/// <param name="Duration">The expected duration needed to close the cover</param>
public readonly record struct Close(TimeSpan? Duration = null) : ICommand;