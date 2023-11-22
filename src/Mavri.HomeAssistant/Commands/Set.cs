using System;
using Mavri.Commands;

namespace Mavri.Ha.Commands;

/// <summary>
/// A command to set a value of a thing (e.g. a text, bool, enum)
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
public readonly record struct Set<T>(T Value) : ICommand;