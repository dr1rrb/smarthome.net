using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Mqtt
{
	internal class MqttCache : IDisposable
	{
		private static readonly char[] _seperator = {'/'};
		
		private int _pendingInitializers; // even if we should have only one concurrent initializer, be safer

		public MqttCache(IMqttConnection connection)
		{
			Root = new MqttCacheTopic(connection);
		}

		public MqttCacheTopic Root { get; }

		public bool TryUpdate(string topic, string value, bool retained)
		{
			this.Log().Debug($"Received MQTT message '{topic}': {value}");

			return topic
				.Split(_seperator, StringSplitOptions.RemoveEmptyEntries)
				.Aggregate(Root, (level, levelName) => level.GetChild(levelName))
				.TryUpdate(value, retained);
		}

		public MqttCacheTopic Get(string topic)
		{
			return topic
				.Split(_seperator, StringSplitOptions.RemoveEmptyEntries)
				.Aggregate(Root, (level, levelName) => level.GetChild(levelName));
		}

		/// <inheritdoc />
		public void Dispose() => Root.Dispose();
	}
}