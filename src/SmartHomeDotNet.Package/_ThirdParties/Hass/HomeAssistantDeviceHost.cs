using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Mavri.Ha.Api;
using SmartHomeDotNet.Hass.Commands;
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
		private readonly HomeAssistantHttpApi _api;

		private ImmutableList<ICommandAdapter> _commands = ImmutableList.Create<ICommandAdapter>(new LockAdapter(), new CoverAdapter());

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="baseTopic">The base topic used by the Home Assistant installation</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public HomeAssistantDeviceHost(MqttClient mqtt, string baseTopic, HomeAssistantHttpApi api, IScheduler scheduler)
			: base(mqtt, device => GetId(device).ToMqttTopic(baseTopic), topic => topic.Values, scheduler)
		{
			_api = api;
		}

		/// <summary>
		/// Registers a custom command adapter on this device host
		/// </summary>
		/// <param name="adapter">The adapter which adapts a generic <see cref="ICommand"/> to a <see cref="CommandData"/> which can be sent to the Home Assistant hub</param>
		public void RegisterCommand(ICommandAdapter adapter)
			=> ImmutableInterlocked.Update(ref _commands, cmds => cmds.Add(adapter));

		private static EntityId GetId(IDevice device)
			=> device.Id is EntityId entityId
				? entityId
				: throw new NotSupportedException($"Device with id '{device.Id}' is not a device from Home Assistant.");

		/// <inheritdoc />
		public override object GetId(object rawId)
			=> EntityId.Parse(rawId);

		/// <inheritdoc />
		public override AsyncContextOperation Execute(ICommand command, IDevice device)
			=> ExecuteCore(command, GetId(device).Component, new[] {device});

		/// <inheritdoc />
		public override AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
		{
			var requests = devices
				.GroupBy(device => GetId(device).Component)
				.Select(comp => ExecuteCore(command, comp.Key, comp));

			return AsyncContextOperation.WhenAll(requests);
		}

		private AsyncContextOperation ExecuteCore(ICommand command, Component component, IEnumerable<IDevice> devices)
		{
			foreach (var adapter in _commands)
			{
				if (adapter.TryGetData(component, command, out var data))
				{
					return AsyncContextOperation.StartNew(async ct =>
					{
						await _api.CallService(data.Domain, data.Service, data.Data.Add(devices), ct);
						if (data.Transition is not null) await Task.Delay(data.Transition.Value, ct);
					});
				}
			}

			// It's acceptable to convert from component to domain here since below we are only supporting core components/domain
			string domain = component;
			
			switch (command)
			{
				case TurnOn on:
					return AsyncContextOperation.StartNew(async ct =>
					{
						await _api.CallService(domain, "turn_on", on.ToParameters(domain, devices, out var tOn), ct);
						if (tOn is not null) await Task.Delay(tOn.Value, ct);
					});
				case TurnOff off:
					return AsyncContextOperation.StartNew(async ct =>
					{
						await _api.CallService(domain, "turn_off", off.ToParameters(domain, devices, out var tOff), ct);
						if (tOff is not null) await Task.Delay(tOff.Value, ct);
					});
				case Toggle toggle:
					return AsyncContextOperation.StartNew(async ct =>
					{
						await _api.CallService(domain, "toggle", toggle.ToParameters(domain, devices, out var tTog), ct);
						if (tTog is not null) await Task.Delay(tTog.Value, ct);
					});
				case ISelectCommand select:
					return AsyncContextOperation.StartNew(ct => _api.CallService(domain, "select_option", select.ToParameters(domain, devices), ct));
				case SetSpeed setSpeed:
					return AsyncContextOperation.StartNew(ct => _api.CallService(domain, "set_speed", setSpeed.ToParameters(domain, devices), ct));
				case SetText setText:
					return AsyncContextOperation.StartNew(ct => _api.CallService(domain, "set_value", setText.ToParameters(domain, devices), ct));

				default:
					throw new NotSupportedException($"Command {command.GetType()} is not supported.");
			}
		}
	}
}
