using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct EntityState<T>(T? Value, EntityState Raw)
{
	public bool IsUnavailable => Raw.IsUnavailable;

	public bool IsUnknown => Raw.IsUnknown;
}