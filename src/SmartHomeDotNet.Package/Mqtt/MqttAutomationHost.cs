using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.SmartHome.Automations;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// An <see cref="IAutomationHost"/> which uses MQTT to communicate with the smart home hub
	/// </summary>
	public sealed class MqttAutomationHost : IAutomationHost
	{
		private static readonly Regex _invalidChars = new Regex("\\W", RegexOptions.Compiled);

		private readonly MqttClient _mqtt;
		private readonly string _baseTopic;
		private readonly bool _enableHomeAssistantDiscovery;

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		public MqttAutomationHost(
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
		public async Task Initialized(CancellationToken ct, Automation automation)
		{
			if (!_enableHomeAssistantDiscovery)
			{
				return;
			}

			var id = "automation_" + GetId(automation);
			var config = new HomeAssistantConfig
			{
				Id = id,
				Name = id, // cf. comment below
				StateTopic = GetStateTopic(automation),
				StateOn = "enabled",
				StateOff = "disabled",
				IsOptimistic = true, // TODO: Add ability for automation to confirm state, then add control channel and set it pessimistic
				CommandTopic = GetStateTopic(automation),
				PayloadOn = "enabled",
				PayloadOff = "disabled",
				IsRetained = true,
				AvailabilityTopic = _mqtt.AvailabilityTopic,
				Icon = "mdi:clipboard-text-play" // "mdi:script-text-outline"
			};

			// With first publish the automation using the "id" as "name" so Home assistant
			// will use it as "entity_id" (which cannot be configured from discovery component)
			// Then we publish it a second time using the right name.
			// Note: HA will not generate 2 different devices as we are providing de vice "unique_id" which stays the same.
			// Note: This is a patch which works only if HA is up when this config is published, if not, you can still change the entity_id from the UI

			await _mqtt.Publish(ct, $"homeassistant/switch/{id}/config", JsonConvert.SerializeObject(config), retain: !_mqtt.IsTestEnvironment);
			config.Name = automation.Name;
			await _mqtt.Publish(ct, $"homeassistant/switch/{id}/config", JsonConvert.SerializeObject(config), retain: !_mqtt.IsTestEnvironment);

			// When we publish devices, Home assistant assume them as enabled.
			// Here we only republish the current state if any.
			var currentState = await IsAutomationEnabled(automation).FirstAsync().ToTask(ct);
			await _mqtt.Publish(ct, GetStateTopic(automation), currentState ? "enabled" : "disabled", QualityOfService.AtLeastOnce, retain: !_mqtt.IsTestEnvironment);
		}

		/// <inheritdoc />
		public IObservable<bool> GetAndObserveIsEnabled(Automation automation)
		{
			var isClientEnabled = _mqtt.GetAndObserveIsEnabled();
			var isAutomationEnabled = IsAutomationEnabled(automation);

			return Observable
				.CombineLatest(isClientEnabled, isAutomationEnabled, (c, a) => c & a)
				.DistinctUntilChanged();
		}

		private IObservable<bool> IsAutomationEnabled(Automation automation)
			=> _mqtt
				.GetAndObserveTopic(GetTopic(automation))
				.Select(topic =>
				{
					var s = topic.Values.GetValueOrDefault("state");

					return s == null || s == "enabled"; // If not configured, we assumed enabled
				});

		// TODO: Cache those
		private string GetId(Automation automation) => _invalidChars.Replace(automation.Id, "_").ToLowerInvariant();
		private string GetStateTopic(Automation automation) => GetTopic(automation, "state");
		private string GetTopic(Automation automation, string subLevel) => $"{_baseTopic}/automation/{GetId(automation)}/{subLevel}";
		private string GetTopic(Automation automation) => $"{_baseTopic}/automation/{GetId(automation)}";
	}
}