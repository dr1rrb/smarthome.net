using System;
using System.Linq;

namespace Mavri.Commands;

/// <summary>
/// Marker interface for commands that can be sent to a <see cref="IThing{TState}"/> using an <see cref="IActuator"/>.
/// </summary>
public interface ICommand
{
}