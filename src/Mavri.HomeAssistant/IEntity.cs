using System;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha;

public interface IEntity : IThingInfo<EntityId>
{
	void Publish(EntityState state);
}