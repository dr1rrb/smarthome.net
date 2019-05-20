using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Text;
using Newtonsoft.Json.Linq;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Zigbee2Mqtt
{
	/// <summary>
	/// A hub which aggregates common services accessible on a Zigbee2Mqtt installation
	/// </summary>
	public class Zigbee2MqttHub : IDisposable
	{
		public const string DefaultTopic = "zigbee2mqtt";

		/// <summary>
		/// Creates an instance of a Zigbee 2 MQTT hub
		/// </summary>
		/// <param name="mqtt">A client to the MQTT broker used by Zigbee 2 MQTT</param>
		/// <param name="scheduler">The scheduler used to run scripts, automations and manage devices of this Home Assistant</param>
		public Zigbee2MqttHub(MqttClient mqtt, IScheduler scheduler, string topic = DefaultTopic)
		{
			var deviceHost = new Zigbee2MqttDeviceHost(mqtt, topic, scheduler);

			Devices = new HomeDevicesManager(deviceHost);
		}

		/// <summary>
		/// Gets the device manager of this instance of Home Assistant
		/// </summary>
		public HomeDevicesManager Devices { get; }

		/// <inheritdoc />
		public void Dispose()
			=> Devices.Dispose();
	}
}
