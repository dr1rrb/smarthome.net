using System;
using System.Linq;

namespace Mavri.Ha;

public interface IDeviceRegistry
{
	IDevice? Get(DeviceId id);
}