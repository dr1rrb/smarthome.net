using System;
using System.Linq;

namespace Mavri.Commands;

/// <summary>
/// A <see cref="IThing{TState}"/> that can be actuated.
/// </summary>
public interface IActuatable : IThingInfo
{
	// Note: why not just a Execute on this instead of pushing the actuator: so we are able to properly coerce request on multiple devices!

	/// <summary>
	/// The actuator to use to actuate this thing.
	/// </summary>
	IActuator Actuator { get; }
}