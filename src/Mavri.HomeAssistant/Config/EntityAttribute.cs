using System;
using System.Linq;
using Mavri.Ha.Entities;

namespace Mavri.Ha.Config;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EntityAttribute : Attribute
{
	public EntityId EntityId { get; }
		
	/// <summary>
	/// The type of the <see cref="Entity{T}"/> to use to back that given entity
	/// </summary>
	public Type EntityType { get; }

	public EntityAttribute(string entityId, Type entityType)
	{
		EntityId = entityId;
		EntityType = entityType;
	}
}