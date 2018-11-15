using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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

			var id = GetId(automation);
			var config = new HomeAssistantConfig
			{
				Id = id,
				Name = automation.Name,
				StateTopic = GetStateTopic(automation),
				StateOn = "enabled",
				StateOff = "disabled",
				IsOptimistic = false,
				CommandTopic = GetControlTopic(automation),
				PayloadOn = "start",
				PayloadOff = "stop",
				IsRetained = false,
				AvailabilityTopic = _mqtt.AvailabilityTopic,
				Icon = "mdi:script-text-outline"
			};

			await _mqtt.Publish(ct, $"homeassistant/switch/{id}/config", JsonConvert.SerializeObject(config), retain: true);
		}

		/// <inheritdoc />
		public IObservable<bool> GetAndObserveIsEnabled(Automation automation)
			=> _mqtt
				.GetAndObserveState(GetStateTopic(automation))
				.Select(state => state.Values.GetValueOrDefault("state") == "enabled")
				.DistinctUntilChanged();

		// TODO: Cache those
		private string GetId(Automation automation) => _invalidChars.Replace(automation.Name, "_").ToLowerInvariant();
		private string GetStateTopic(Automation automation) => GetTopic(automation, "state");
		private string GetControlTopic(Automation automation) => GetTopic(automation, "control");
		private string GetTopic(Automation automation, string subLevel) => $"{_baseTopic}/{GetId(automation)}/{subLevel}";
	}
}