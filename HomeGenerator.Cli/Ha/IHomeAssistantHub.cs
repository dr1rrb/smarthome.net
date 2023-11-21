using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace Mavri.Ha;

public interface IHomeAssistantHub : IDeviceActuator<EntityId>, IDeviceActuator<DeviceId>
{
	/// <summary>
	/// The base (http[s]) uri of this home-assistant instance.
	/// </summary>
	public Uri BaseUri { get; }

	/// <summary>
	/// Registry of entities hosted by this home-assistant instance.
	/// </summary>
	public IEntityRegistry Entities { get; }

	/// <summary>
	/// Registry of devices hosted by this home-assistant instance.
	/// </summary>
	public IDeviceRegistry Devices { get; }
}