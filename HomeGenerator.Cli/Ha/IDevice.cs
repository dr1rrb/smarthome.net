using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices;

namespace Mavri.Ha;

public interface IDevice : IThingInfo<DeviceId>
{
}