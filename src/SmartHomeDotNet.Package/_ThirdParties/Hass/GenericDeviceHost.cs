using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using SmartHomeDotNet.Hass.Commands;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	///// <summary>
	///// A <see cref="IDeviceHost"/> that allow communication with devices managed by a Home Assistant installation over MQTT
	///// </summary>
	//public sealed class HomeAssistantDeviceHost : MqttDeviceHost
	//{
	//	private readonly HomeAssistantApi _api;

	//	/// <summary>
	//	/// Creates a new instance
	//	/// </summary>
	//	/// <param name="mqtt">A client to an MQTT broker</param>
	//	/// <param name="baseTopic">The base topic used by the Home Assistant installation</param>
	//	/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
	//	public HomeAssistantDeviceHost(MqttClient mqtt, string baseTopic, HomeAssistantApi api, IScheduler scheduler)
	//		: base(mqtt, device => GetId(device).ToMqttTopic(baseTopic), topic => topic.Values, scheduler)
	//	{
	//		_api = api;
	//	}

	//	private static EntityId GetId(IDevice device)
	//		=> device.Id is EntityId entityId
	//			? entityId
	//			: throw new NotSupportedException($"Device with id '{device.Id}' is not a device from Home Assistant.");

	//	/// <inheritdoc />
	//	public override object GetId(object rawId)
	//		=> EntityId.Parse(rawId);

	//	/// <inheritdoc />
	//	public override AsyncContextOperation Execute(ICommand command, IDevice device)
	//		=> ExecuteCore(command, GetId(device).Component, new[] {device});

	//	/// <inheritdoc />
	//	public override AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
	//	{
	//		var requests = devices
	//			.GroupBy(device => GetId(device).Component)
	//			.Select(comp => ExecuteCore(command, comp.Key, comp));

	//		return AsyncContextOperation.WhenAll(requests);
	//	}

	//	private AsyncContextOperation ExecuteCore(ICommand command, Component component, IEnumerable<IDevice> devices)
	//	{
	//		var comp = component.ToString().ToLowerInvariant();

	//		switch (command)
	//		{
	//			case TurnOn on:
	//				return _api.Execute(comp, "turn_on", on.ToParameters(component, devices, out var tOn), tOn);
	//			case TurnOff off:
	//				return _api.Execute(comp, "turn_off", off.ToParameters(component, devices, out var tOff), tOff);
	//			case Toggle toggle:
	//				return _api.Execute(comp, "toggle", toggle.ToParameters(component, devices, out var tTog), tTog);
	//			case ISelectCommand select:
	//				return _api.Execute(comp, "select_option", select.ToParameters(component, devices));
	//			case SetSpeed setSpeed:
	//				return _api.Execute(comp, "set_speed", setSpeed.ToParameters(component, devices));

	//			default:
	//				throw new NotSupportedException($"Command {command.GetType()} is not supported.");
	//		}
	//	}
	//}

	public class GenericDeviceHost<TDeviceId> : IDeviceHost
	{
		private readonly HomeDevicesManager<TDeviceId> _devices;

		public GenericDeviceHost(IDeviceStateProvider<TDeviceId> state, IDeviceActuator actuator, IScheduler scheduler)
		{
			Actuator = actuator;
			Scheduler = scheduler;

			_devices = new HomeDevicesManager<TDeviceId>(this, state);
		}

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public HomeDevice<TDevice> Get<TDevice>(object rawId)
			where TDevice : IDeviceAdapter, new()
			=> _devices.GetDevice<TDevice>(rawId);

		/// <inheritdoc />
		public IDeviceActuator Actuator { get; }
	}

	internal class HomeAssistantMqttStateProvider : IDeviceStateProvider<EntityId>
	{
		private readonly MqttClient _mqtt;
		private readonly string _baseTopic;

		public HomeAssistantMqttStateProvider(MqttClient mqtt, string baseTopic)
		{
			_mqtt = mqtt;
			_baseTopic = baseTopic;
		}

		/// <inheritdoc />
		public EntityId Parse(object rawId)
			=> EntityId.Parse(rawId);

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState(EntityId id)
			=> _mqtt
				.GetAndObserveTopic(id.ToMqttTopic(_baseTopic))
				.Select(topic => new DeviceState(id, topic.Values, topic.IsRetainedState));
	}

	internal class HomeAssistantApiDeviceActuator : IDeviceActuator
	{
		private readonly HomeAssistantApi _api;
		private ImmutableList<ICommandAdapter> _commands = ImmutableList<ICommandAdapter>.Empty;

		public HomeAssistantApiDeviceActuator(HomeAssistantApi api)
		{
			_api = api;
		}

		/// <summary>
		/// Registers a custom command adapter on this device host
		/// </summary>
		/// <param name="adapter">The adapter which adapts a generic <see cref="ICommand"/> to a <see cref="CommandData"/> which can be sent to the Home Assistant hub</param>
		public void RegisterCommand(ICommandAdapter adapter)
			=> ImmutableInterlocked.Update(ref _commands, cmds => cmds.Add(adapter));

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IDevice device)
			=> ExecuteCore(command, GetId(device).Component, new[] { device });

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
		{
			var requests = devices
				.GroupBy(device => GetId(device).Component)
				.Select(comp => ExecuteCore(command, comp.Key, comp));

			return AsyncContextOperation.WhenAll(requests);
		}

		private AsyncContextOperation ExecuteCore(ICommand command, Component domain, IEnumerable<IDevice> devices)
		{
			string comp = domain;

			foreach (var adapter in _commands)
			{
				if (adapter.TryGetData(domain, command, out var data))
				{
					return _api.Execute(data.Component, data.Service, data.Parameters.Add(devices), data.Transition);
				}
			}

			switch (command)
			{
				case TurnOn on:
					return _api.Execute(comp, "turn_on", on.ToParameters(domain, devices, out var tOn), tOn);
				case TurnOff off:
					return _api.Execute(comp, "turn_off", off.ToParameters(domain, devices, out var tOff), tOff);
				case Toggle toggle:
					return _api.Execute(comp, "toggle", toggle.ToParameters(domain, devices, out var tTog), tTog);
				case ISelectCommand select:
					return _api.Execute(comp, "select_option", select.ToParameters(domain, devices));
				case SetSpeed setSpeed:
					return _api.Execute(comp, "set_speed", setSpeed.ToParameters(domain, devices));
				case SetText setText:
					return _api.Execute(comp, "set_value", setText.ToParameters(domain, devices));

				default:
					throw new NotSupportedException($"Command {command.GetType()} is not supported.");
			}
		}

		private static EntityId GetId(IDevice device)
			=> device.Id is EntityId entityId
				? entityId
				: throw new NotSupportedException($"Device with id '{device.Id}' is not a device from Home Assistant.");
	}
}
