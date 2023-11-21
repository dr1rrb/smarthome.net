using System;
using System.Linq;
using Mavri.Ha.Data;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices;

namespace Mavri.Ha;

public interface IEntity : IThingInfo<EntityId>
{
	void Publish(EntityState state);
}