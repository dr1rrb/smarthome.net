using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Utils;

internal class EntityIdComparer : IEqualityComparer<EntityData>
{
	public static EntityIdComparer Instance { get; } = new();

	/// <inheritdoc />
	public bool Equals(EntityData? x, EntityData? y)
		=> (x, y) switch
		{
			(null, null) => true,
			(null, _) => false,
			(_, null) => false,
			_ => x.Id.Equals(y.Id)
		};

	/// <inheritdoc />
	public int GetHashCode(EntityData obj)
		=> obj.Id.GetHashCode();
}