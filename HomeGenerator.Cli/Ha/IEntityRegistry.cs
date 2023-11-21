using System;
using System.Linq;
using Mavri.Ha.Entities;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha;

public interface IEntityRegistry
{
	IEntity? Get(EntityId id);
}