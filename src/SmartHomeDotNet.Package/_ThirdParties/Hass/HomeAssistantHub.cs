using System;
using System.Linq;
using System.Reactive.Concurrency;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// A hub which aggregates common services accessible on a Home Assistant installation
	/// </summary>
	public class HomeAssistantHub : IDisposable
	{
		public const string DefaultHomeTopic = "smarthomedotnet";
		public const string DefaultTopic = "homeassistant";

		/// <summary>
		/// Creates an instance of a Home Assistant hub which will use MQTT state stream
		/// (cf. <seealso cref="https://www.home-assistant.io/components/mqtt_statestream/"/>) to read states of devices,
		/// and the REST API (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_rest.html"/>)
		/// to write state / execute services on the hub.
		/// </summary>
		/// <param name="scheduler">The scheduler used to run scripts, automations and manage devices of this Home Assistant</param>
		/// <param name="apiHostName">The host name (and port) of the Home Assistant API (&lt;host&gt;[:port])</param>
		/// <param name="apiPassword">The API password defined in Home Assistant configuration</param>
		/// <param name="mqtt">A client to the MQTT broker used by Home Assistant</param>
		/// <param name="homeTopic">Defines the base topic that will be used for to publish scenes and automations</param>
		/// <param name="hassTopic">Defines the base topic used by home assistant state stream</param>
		public HomeAssistantHub(
			IScheduler scheduler, 
			string apiHostName,
			string apiPassword,
			MqttClient mqtt, 
			string homeTopic = DefaultHomeTopic, 
			string hassTopic = DefaultTopic)
		{
			homeTopic = homeTopic.Trim('/', '#', '*');
			hassTopic = hassTopic.Trim('/', '#', '*');

			var api = new HomeAssistantApi(apiHostName, apiPassword);
			var mqttStateStream = new HomeAssistantDeviceHost(mqtt, hassTopic, api, scheduler);

			Devices = new HomeDevicesManager(mqttStateStream);
			Scenes = new MqttSceneHost(mqtt, homeTopic, scheduler);
			Automations = new MqttAutomationHost(mqtt, homeTopic, scheduler);
			Api = api;
		}

		/// <summary>
		/// Gets the scenes host of this instance of Home Assistant
		/// </summary>
		public ISceneHost Scenes { get; }

		/// <summary>
		/// Gets the automation host of this instance of Home Assistant
		/// </summary>
		public IAutomationHost Automations { get; }

		/// <summary>
		/// Gets the device manager of this instance of Home Assistant
		/// </summary>
		public HomeDevicesManager Devices { get; }

		/// <summary>
		/// Gets the API of this instance of Home Assistant
		/// </summary>
		public HomeAssistantApi Api { get; }

		/// <inheritdoc />
		public void Dispose()
			=> Devices.Dispose();
	}
}