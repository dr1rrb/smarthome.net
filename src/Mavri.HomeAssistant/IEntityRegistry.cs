using System;
using System.Linq;

namespace Mavri.Ha;

public interface IEntityRegistry
{
	IEntity? Get(EntityId id);
}