using System;
using System.Linq;

namespace Mavri.Commands;

/// <summary>
/// Something that can actuate a <see cref="IThing{TIdentifier, TState}"/>.
/// </summary>
public interface IActuator<in TIdentifier> : IActuator
{
	/// <summary>
	/// Executes a command on the target device
	/// </summary>
	/// <param name="command">The command to execute</param>
	/// <param name="devices">The devices on which the command have to be executed</param>
	/// <returns>An async operation</returns>
	Task Execute(ICommand command, params TIdentifier[] devices);

	/// <inheritdoc cref="IActuator"/>
	Task IActuator.Execute(ICommand command, params object[] things)
		=> Execute(command, things.Cast<TIdentifier>().ToArray());
}