using System;
using System.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Devices;

namespace Mavri.Ha;

// For now the device is only an aggregation of entities, in a future we will try to determine the "main" entity and implement IThing<DeviceId, GenDevice> where GenDevice : {MainEntityState}, ISupport<{MainEntityCommands}>
public abstract record Device(DeviceId Id, IHomeAssistantHub Hub) : IThingInfo<DeviceId>, IDevice;