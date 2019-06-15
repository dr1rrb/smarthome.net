using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Text;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> that allow communication with devices managed by a Home Assistant installation over MQTT
	/// </summary>
	public sealed class HomeAssistantDeviceHost : MqttDeviceHost
	{
		private readonly HomeAssistantApi _api;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="baseTopic">The base topic used by the Home Asssistant installation</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public HomeAssistantDeviceHost(MqttClient mqtt, string baseTopic, HomeAssistantApi api, IScheduler scheduler)
			: base(mqtt, device => baseTopic + "/" + ((string)device.Id).Replace('.','/'), topic => topic.Values, scheduler)
		{
			_api = api;
		}

		/// <inheritdoc />
		public override AsyncContextOperation Execute(ICommand command, IDevice device)
		{
			if (!(device.Id is EntityId deviceId))
			{
				throw new NotSupportedException($"Device with id '{device.Id}' is not a device from Home Assistant.");
			}

			return ExecuteCore(command, deviceId.Component, new[] {device});
		}

		/// <inheritdoc />
		public override AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
		{
			var devicePerComponent = devices.GroupBy(device =>
			{
				if (!(device.Id is EntityId deviceId))
				{
					throw new NotSupportedException($"Device with id '{device.Id}' is not a device from Home Assistant.");
				}

				return deviceId.Component;
			});

			throw new NotImplementedException("Command host is still in progress, please use the API");

			//ApiCall Send(IGrouping<Component, IDevice> componentDevices)
			//	=> ExecuteCore(command, componentDevices.Key, componentDevices);
		}

		private ApiCall ExecuteCore(ICommand command, Component component, IEnumerable<IDevice> devices)
		{
			var comp = component.ToString().ToLowerInvariant();

			switch (command)
			{
				// Turn On
				case TurnOn on:
					return _api.Execute(comp, "turn_on", on.ToParameters(component, devices));

				// Turn off
				case TurnOff off:
					return _api.Execute(comp, "turn_off", off.ToParameters(component, devices));

				// Toggle
				case Toggle toggle:
					return _api.Execute(comp, "toggle", toggle.ToParameters(component, devices));

				default:
					throw new NotSupportedException($"Command {command.GetType()} is not supported.");
			}
		}
	}
}
