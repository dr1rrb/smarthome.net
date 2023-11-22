using System;
using System.Linq;

namespace Mavri.Commands;

/// <summary>
/// Marker interface for an <see cref="IThing{TState}"/> to indicates the supported <see cref="ICommand"/>
/// </summary>
/// <typeparam name="T">Type of the command that is supported by this device</typeparam>
public interface ISupport<T> : IActuatable
	where T : ICommand
{
}