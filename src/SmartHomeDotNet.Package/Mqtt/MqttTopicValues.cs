using System;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// An immutable snapshot of values of a MQTT topic
	/// </summary>
	public class MqttTopicValues
	{
		/// <summary>
		/// Creates  anew instance
		/// </summary>
		/// <param name="topic">The owning topic</param>
		/// <param name="values">The current values of the topic</param>
		public MqttTopicValues(string topic, ImmutableDictionary<string, string> values)
		{
			Topic = topic;
			Values = values;
		}


		/// <summary>
		/// The topic of this values
		/// </summary>
		public string Topic { get; }

		/// <summary>
		/// The current values of the topic
		/// </summary>
		public ImmutableDictionary<string, string> Values { get; }
	}
}