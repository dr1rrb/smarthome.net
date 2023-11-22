#nullable enable
using System;
using System.Linq;

namespace Mavri;

/// <summary>
/// A T of IoT.
/// </summary>
/// <typeparam name="TState">Type of the state of this thing.</typeparam>
public interface IThing<out TState> : IObservable<TState>, IThingInfo
{
}