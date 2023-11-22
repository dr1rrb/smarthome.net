using System;
using System.Linq;

namespace Mavri;

/// <summary>
/// Metadata of a <see cref="IThing{TState}"/>.
/// </summary>
public interface IThingInfo
{
	/// <summary>
	/// The identifier of the thing.
	/// </summary>
	object Id { get; }
}