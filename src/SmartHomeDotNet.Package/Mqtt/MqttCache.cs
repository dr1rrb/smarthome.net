using System;
using System.Linq;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Mqtt
{
	internal class MqttCache
	{
		private static readonly char[] _seperator = {'/'};

		public MqttCache()
		{
			Root = new MqttTopic();
		}

		public MqttTopic Root { get; }

		public (bool hasUpdated, MqttTopic updated) TryUpdate(string topic, string value, bool retained)
		{
			this.Log().Info($"Received MQTT message '{topic}': {value}");

			var updated = topic
				.Split(_seperator, StringSplitOptions.RemoveEmptyEntries)
				.Aggregate(Root, (level, levelName) => level.GetChild(levelName));
			var hasUpdated = updated.TryUpdate(value, retained);

			return (hasUpdated, updated);
		}

		public MqttTopic Get(string topic)
		{
			return topic
				.Split(_seperator, StringSplitOptions.RemoveEmptyEntries)
				.Aggregate(Root, (level, levelName) => level.GetChild(levelName));
		}
	}
}