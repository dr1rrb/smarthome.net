#nullable enable

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// A hub which aggregates common services accessible on a Home Assistant installation
	/// </summary>
	public class HomeAssistantHub : IDisposable
	{
		public const string DefaultHomeTopic = "smarthomedotnet";
		public const string DefaultTopic = "homeassistant";

		private readonly HomeAssistantDeviceHost _mqttStateStream;

		/// <summary>
		/// Creates an instance of a Home Assistant hub which will use MQTT state stream
		/// (cf. <seealso cref="https://www.home-assistant.io/components/mqtt_statestream/"/>) to read states of devices,
		/// and the REST API (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_rest.html"/>)
		/// to write state / execute services on the hub.
		/// </summary>
		/// <param name="scheduler">The scheduler used to run scripts, automations and manage devices of this Home Assistant</param>
		/// <param name="apiHostName">The host name (and port) of the Home Assistant API (&lt;host&gt;[:port])</param>
		/// <param name="apiToken">The API token of your home assistant hub</param>
		/// <param name="mqtt">A client to the MQTT broker used by Home Assistant</param>
		/// <param name="homeTopic">Defines the base topic that will be used for to publish scenes and automations</param>
		/// <param name="hassTopic">Defines the base topic used by home assistant state stream</param>
		/// <param name="enableDiscovery">Enabled scenes and automations discovery</param>
		public HomeAssistantHub(
			IScheduler scheduler, 
			string apiHostName,
			string apiToken,
			MqttClient mqtt, 
			string homeTopic = DefaultHomeTopic, 
			string hassTopic = DefaultTopic,
			bool enableDiscovery = true)
		{
			homeTopic = homeTopic.Trim('/', '#', '*');
			hassTopic = hassTopic.Trim('/', '#', '*');

			var api = new HomeAssistantHttpApi(apiHostName, apiToken);
			_mqttStateStream = new HomeAssistantDeviceHost(mqtt, hassTopic, api, scheduler);

			Devices = new HomeDevicesManager(_mqttStateStream);
			Scenes = new MqttSceneHost(mqtt, homeTopic, scheduler, enableDiscovery);
			Automations = new MqttAutomationHost(mqtt, homeTopic, scheduler, enableDiscovery);
			Api = api;
			SocketApi = new HomeAssistantWebSocketApi(new Uri($"ws://{apiHostName}"), apiToken);
		}

		/// <summary>
		/// Registers a custom command adapter on the device host of this hub
		/// </summary>
		/// <param name="adapter">The adapter which adapts a generic <see cref="ICommand"/> to a <see cref="CommandData"/> which can be sent to this hub</param>
		/// <returns>The current hub to re-use for fluent declaration</returns>
		public HomeAssistantHub RegisterCommand(ICommandAdapter adapter)
		{
			_mqttStateStream.RegisterCommand(adapter);
			return this;
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
		public HomeAssistantHttpApi Api { get; }

		/// <summary>
		/// Gets the websocket API of this instance of Home Assistant
		/// </summary>
		public HomeAssistantWebSocketApi SocketApi { get; }

		/// <summary>
		/// Call a service on Home-Assistant
		/// </summary>
		/// <param name="command">The call service command to send to HA</param>
		/// <returns></returns>
		public AsyncContextOperation Send(CallServiceCommand command) => Send(command as HomeAssistantCommand);

		internal AsyncContextOperation Send(HomeAssistantCommand command)
		{
			if (command is CallServiceCommand callService)
			{
				if (SocketApi.IsConnected(out var connection))
				{
					return callService.Transition.HasValue
						? AsyncContextOperation.StartNew(Send, Extent)
						: AsyncContextOperation.StartNew(Send);

					async Task Send(CancellationToken ct)
					{
						using (connection)
						{
							await SocketApi.Send(command, ct);
						}
					}

					async Task Extent(CancellationToken ct)
					{
						await Task.Delay(callService.Transition.Value);
					}
				}
				else
				{
					return Api.CallService(callService.Domain, callService.Service, callService.Data, callService.Transition);
				}
			}
			else
			{
				return AsyncContextOperation.StartNew(async ct => await SocketApi.Send(command, ct));
			}
		}

		/// <inheritdoc />
		public void Dispose()
			=> Devices.Dispose();
	}
}