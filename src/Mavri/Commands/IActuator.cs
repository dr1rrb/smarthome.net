using System;
using System.Linq;

namespace Mavri.Commands;

/// <summary>
/// Something that can actuate a <see cref="IThing{TState}"/>.
/// </summary>
public interface IActuator
{
	/// <summary>
	/// Executes a command on the target thing.
	/// </summary>
	/// <param name="command">The command to execute.</param>
	/// <param name="things">The things on which the command have to be executed.</param>
	/// <returns>An async operation</returns>
	Task Execute(ICommand command, params object[] things);
}