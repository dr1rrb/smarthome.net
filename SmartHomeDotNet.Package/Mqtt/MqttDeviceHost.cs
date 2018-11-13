using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Mqtt
{
	public sealed class MqttDeviceHost : IDeviceHost
	{
		private readonly MqttClient _mqtt;
		private readonly Func<string, string> _deviceIdToTopic;

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		public MqttDeviceHost(
			MqttClient mqtt, 
			Func<string, string> deviceIdToTopic, 
			IScheduler scheduler)
		{
			Scheduler = scheduler;
			_mqtt = mqtt;
			_deviceIdToTopic = deviceIdToTopic;
		}

		public IObservable<DeviceState> GetAndObserveState(IDevice device) 
			=> _mqtt
				.GetAndObserveState(_deviceIdToTopic(device.Id))
				.Select(topic => new DeviceState(device.Id, topic.Values));
	}
}