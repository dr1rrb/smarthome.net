using System;
using System.Linq;
using SmartHomeDotNet.Hass;

namespace Mavri.Ha;

public interface IDeviceRegistry
{
	IDevice? Get(DeviceId id);
}