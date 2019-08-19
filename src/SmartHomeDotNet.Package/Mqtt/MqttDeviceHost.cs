using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// A <see cref="IDeviceHost"/> which relies on MQTT to communicate with devices
	/// </summary>
	public abstract class MqttDeviceHost : IDeviceHost
	{
		private readonly MqttClient _mqtt;
		private readonly Func<IDevice, string> _getTopic;
		private readonly Func<MqttTopicValues, ImmutableDictionary<string, string>> _getValues;

		/// <inheritdoc />
		public IScheduler Scheduler { get; }


		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="getTopic">A delegate to determine the MQTT topic to use for a given device</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public MqttDeviceHost(
			MqttClient mqtt,
			Func<IDevice, string> getTopic,
			IScheduler scheduler)
			: this(mqtt, getTopic, topic => topic.Values, scheduler)
		{
		}

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="mqtt">A client to an MQTT broker</param>
		/// <param name="getTopic">A delegate to determine the MQTT topic to use for a given device</param>
		/// <param name="getValues">A delegate to get the values of a device from the current values of MQTT topic</param>
		/// <param name="scheduler">The scheduler that is used by devices which uses this host</param>
		public MqttDeviceHost(
			MqttClient mqtt,
			Func<IDevice, string> getTopic,
			Func<MqttTopicValues, ImmutableDictionary<string, string>> getValues,
			IScheduler scheduler)
		{
			_mqtt = mqtt;
			_getTopic = getTopic;
			_getValues = getValues;

			Scheduler = scheduler;
		}

		/// <inheritdoc />
		public abstract object GetId(object rawId);

		/// <inheritdoc cref="IDeviceHost"/>
		public IObservable<DeviceState> GetAndObserveState(IDevice device)
			=> _mqtt
				.GetAndObserveTopic(_getTopic(device))
				.Select(topic => new DeviceState(device.Id, _getValues(topic), topic.IsRetainedState));

		/// <inheritdoc />
		public abstract AsyncContextOperation Execute(ICommand command, IDevice device);

		/// <inheritdoc />
		public abstract AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices);
	}
}