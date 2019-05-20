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
	/// A <see cref="IDeviceHost"/> that allow communication with devices managed by a Zigbee 2 MQTT installation
	/// </summary>
	public sealed class Zigbee2MqttDeviceHost : MqttDeviceHost
	{
		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="baseTopic">The base topic used by the Zigbee 2 MQTT installation</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public Zigbee2MqttDeviceHost(MqttClient mqtt, string baseTopic, IScheduler scheduler)
			: base(mqtt, device => baseTopic + "/" + device.Id, ParseValues, scheduler)
		{
		}

		private static ImmutableDictionary<string, string> ParseValues(MqttTopicValues topic)
		{
			if (topic.Value == null)
			{
				return ImmutableDictionary<string, string>.Empty;
			}

			var rawValues = (IEnumerable<KeyValuePair<string, JToken>>) JObject.Parse(topic.Value);
			var values = rawValues.ToImmutableDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value is JValue value
						? value.ToString(CultureInfo.InvariantCulture)
						: kvp.Value.ToString());

			return values;
		}
	}
}
