using System;
using System.Linq;

namespace HomeGenerator.Cli;

public record struct EntityState<T>(T? Value, EntityState Raw)
{
	public bool IsUnavailable => Raw.IsUnavailable;

	public bool IsUnknown => Raw.IsUnknown;
}