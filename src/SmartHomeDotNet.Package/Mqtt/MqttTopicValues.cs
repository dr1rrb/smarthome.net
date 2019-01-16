using System;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// An immutable snapshot of values of a MQTT topic
	/// </summary>
	public class MqttTopicValues : IEquatable<MqttTopicValues>
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

		/// <inheritdoc />
		public override int GetHashCode() => Topic.GetHashCode() ^ Values.Count;

		/// <inheritdoc />
		public override bool Equals(object obj) => Equals(this, obj as MqttTopicValues);

		/// <inheritdoc />
		public bool Equals(MqttTopicValues other) => Equals(this, other);

		private static bool Equals(MqttTopicValues left, MqttTopicValues right)
		{
			if (object.ReferenceEquals(left, right))
			{
				return true;
			}

			if (object.ReferenceEquals(null, left))
			{
				return object.ReferenceEquals(null, right);
			}

			return left.Topic.Equals(right.Topic, StringComparison.OrdinalIgnoreCase)
				&& left.Values.SequenceEqual(right.Values);
		}

		public static bool operator ==(MqttTopicValues left, MqttTopicValues right) => Equals(left, right);

		public static bool operator !=(MqttTopicValues left, MqttTopicValues right) => !Equals(left, right);
	}
}