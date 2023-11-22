using System;
using System.Linq;

namespace Mavri;

/// <summary>
/// A T of IoT.
/// </summary>
/// <typeparam name="TIdentifier">Type of the identifier of this thing.</typeparam>
/// <typeparam name="TState">Type of the state of this thing.</typeparam>
public interface IThing<out TIdentifier, out TState> : IThing<TState>, IThingInfo<TIdentifier>
	where TIdentifier : notnull
{
	/// <inheritdoc cref="IThing{TState}"/>
	object IThingInfo.Id => ((IThingInfo<TIdentifier>)this).Id;
}