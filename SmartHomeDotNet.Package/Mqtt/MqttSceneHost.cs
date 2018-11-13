using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Scenes;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// An <see cref="ISceneHost"/> which uses MQTT to communicate with the smart home hub
	/// </summary>
	public sealed class MqttSceneHost : ISceneHost
	{
		private static readonly Regex _invalidChars = new Regex("\\W", RegexOptions.Compiled);

		private readonly MqttClient _mqtt;
		private readonly string _baseTopic;
		private readonly bool _enableHomeAssistantDiscovery;

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		public MqttSceneHost(
			MqttClient mqtt,
			string baseTopic,
			IScheduler scheduler,
			bool enableHomeAssistantDiscovery = true)
		{
			Scheduler = scheduler;
			_mqtt = mqtt;
			_baseTopic = baseTopic;
			_enableHomeAssistantDiscovery = enableHomeAssistantDiscovery;
		}

		/// <inheritdoc />
		public async Task Initialized(CancellationToken ct, Scene scene)
		{
			if (!_enableHomeAssistantDiscovery)
			{
				return;
			}

			var id = GetId(scene);
			var config = new HomeAssistantConfig
			{
				Id = id,
				Name = scene.Name,
				StateTopic = GetStateTopic(scene),
				StateOn = "running",
				StateOff = "idle",
				IsOptimistic = false,
				CommandTopic = GetControlTopic(scene),
				PayloadOn = "start",
				PayloadOff = "stop",
				IsRetained = false,
				AvailabilityTopic = _mqtt.AvailabilityTopic,
				Icon = "mdi:script-text-outline"
			};

			await _mqtt.Publish(ct, $"homeassistant/switch/{id}/config", JsonConvert.SerializeObject(config), retain: true);
		}

		/// <inheritdoc />
		public async Task SetIsRunning(CancellationToken ct, Scene scene, bool isRunning)
		{
			await _mqtt.Publish(ct, GetStateTopic(scene), isRunning ? "running" : "idle");
		}

		/// <inheritdoc />
		public IObservable<SceneCommand> ObserveCommands(Scene scene)
		{
			return _mqtt
				.ObserveEvent(GetControlTopic(scene))
				.Select(evt => evt.Equals("start", StringComparison.OrdinalIgnoreCase)
					? SceneCommand.Start
					: SceneCommand.Stop);
		}

		// TODO: Cache those
		private string GetId(Scene scene) => _invalidChars.Replace(scene.Name, "_").ToLowerInvariant();
		private string GetStateTopic(Scene scene) => GetTopic(scene, "state");
		private string GetControlTopic(Scene scene) => GetTopic(scene, "control");
		private string GetTopic(Scene scene, string subLevel) => $"{_baseTopic}/{GetId(scene)}/{subLevel}";
	}
}