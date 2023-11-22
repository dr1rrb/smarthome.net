using System;
using System.Linq;

namespace Mavri;

/// <summary>
/// Metadata of a <see cref="IThing{TIdentifier, TState}"/>.
/// </summary>
/// <typeparam name="TIdentifier">Type of the identifier of the thing.</typeparam>
public interface IThingInfo<out TIdentifier>
{
	/// <summary>
	/// The identifier of the thing.
	/// </summary>
	TIdentifier Id { get; }
}