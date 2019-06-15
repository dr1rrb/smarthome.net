using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// An entity ID which is use by home assistant to identify devices
	/// </summary>
	public struct EntityId
	{
		public EntityId(Component component, string id)
		{
			Component = component;
			Id = id;
		}

		/// <summary>
		/// The component part of this entity ID (i.e. the part preceding the dot)
		/// </summary>
		public Component Component { get; }

		/// <summary>
		/// The ID part of the entity id (i.e. part following the dot)
		/// </summary>
		public string Id { get; }

		/// <inheritdoc />
		public override string ToString()
			=> Component.ToString().ToLowerInvariant() + '.' + Id;

		public static EntityId Parse(string entityId)
		{
			var separatorIndex = entityId.IndexOf('.');
			if (separatorIndex < 0 || separatorIndex == entityId.Length - 1)
			{
				throw new ArgumentOutOfRangeException(nameof(entityId), "The value is not of format <component>.<id>");
			}

			return new EntityId(
				component: entityId.Substring(0, separatorIndex), 
				id: entityId.Substring(0, separatorIndex + 1));
		}
	}
}
