using System;
using System.Linq;
using Mavri.Ha.Core;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices_2;
using SmartHomeDotNet.Utils;

namespace Mavri.Ha.Generation;

public static class T
{
	public static readonly string HA = "global::" + typeof(IHomeAssistantHub).FullName;
	public static readonly string Actuator = "global::" + typeof(IDeviceActuator).FullName;
	public static readonly string AsyncOp = "global::" + typeof(AsyncContextOperation).FullName;
	public static readonly string ICommand = "global::" + typeof(ICommand).FullName;

	public static readonly string EntityId = "global::" + typeof(EntityId).FullName;
	public static readonly string IEntity = "global::" + typeof(IEntity).FullName;
	public static readonly string IEntityRegistry = "global::" + typeof(IEntityRegistry).FullName;
	//public static readonly string Entity = "global::HomeGenerator.Cli.Entity";

	public static readonly string DeviceId = "global::" + typeof(DeviceId).FullName;
	public static readonly string IDevice = "global::" + typeof(IDevice).FullName;
	public static readonly string IDeviceRegistry = "global::" + typeof(IDeviceRegistry).FullName;
	public static readonly string Device = "global::" + typeof(Device).FullName;


	public static readonly string EntitiesManager = "global::" + typeof(EntitiesUpdater).FullName;
	public static readonly string SocketApi = "global::" + typeof(HomeAssistantWebSocketApi).FullName;
	public static readonly string RestApi = "global::" + typeof(HomeAssistantHttpApi).FullName;
}