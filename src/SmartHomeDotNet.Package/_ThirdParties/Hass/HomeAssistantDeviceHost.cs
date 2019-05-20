using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> that allow communication with devices managed by a Home Assistant installation over MQTT
	/// </summary>
	public sealed class HomeAssistantDeviceHost : MqttDeviceHost
	{
		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="baseTopic">The base topic used by the Home Asssistant installation</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public HomeAssistantDeviceHost(MqttClient mqtt, string baseTopic, IScheduler scheduler)
			: base(mqtt, device => baseTopic + "/" + device.Id.Replace('.', '/'), topic => topic.Values, scheduler)
		{
		}
	}
}
