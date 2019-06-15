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

		Task Subscribe(CancellationToken ct, string topic);
	}
}