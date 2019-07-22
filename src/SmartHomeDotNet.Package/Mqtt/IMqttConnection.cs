using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeDotNet.Mqtt
{
	internal interface IMqttConnection
	{
		IScheduler Scheduler { get; }

		IObservable<MqttConnectionStatus> GetAndObserveStatus();

		Task Subscribe(CancellationToken ct, string topic);
	}

	public enum MqttConnectionStatus
	{
		Disabled,

		Connecting,

		Connected
	}
}