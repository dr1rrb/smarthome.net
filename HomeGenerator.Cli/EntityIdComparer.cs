using System;
using System.Linq;

namespace HomeGenerator.Cli;

internal class EntityIdComparer : IEqualityComparer<EntityInfo>
{
	public static EntityIdComparer Instance { get; } = new();

	/// <inheritdoc />
	public bool Equals(EntityInfo? x, EntityInfo? y)
		=> (x, y) switch
		{
			(null, null) => true,
			(null, _) => false,
			(_, null) => false,
			_ => x.Id.Equals(y.Id)
		};

	/// <inheritdoc />
	public int GetHashCode(EntityInfo obj)
		=> obj.Id.GetHashCode();
}