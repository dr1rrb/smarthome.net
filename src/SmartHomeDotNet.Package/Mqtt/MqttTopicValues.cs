using System;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.Mqtt
{
	/// <summary>
	/// An immutable snapshot of the values of a MQTT topic and its **first** level of sub topics
	/// </summary>
	public class MqttTopicValues : IEquatable<MqttTopicValues>
	{
		/// <summary>
		/// Creates  anew instance
		/// </summary>
		/// <param name="topic">The owning topic</param>
		/// <param name="value">TRhe local value of the topic</param>
		/// <param name="children">The current values of the topic</param>
		public MqttTopicValues(string topic, string value, ImmutableDictionary<string, string> children, bool isRetainedState)
		{
			Topic = topic;
			Value = value;
			Values = children;
			IsRetainedState = isRetainedState;
		}

		/// <summary>
		/// Indicates if any value of this topics or its sub topic was retained
		/// </summary>
		public bool IsRetainedState { get; set; }

		/// <summary>
		/// The topic of this values
		/// </summary>
		public string Topic { get; }

		/// <summary>
		/// The local value of this topic
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// The current values of the sub topics
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
				&& (left.Value?.Equals(right.Value, StringComparison.Ordinal) ?? right.Value == null)
				&& left.Values.SequenceEqual(right.Values);
		}

		public static bool operator ==(MqttTopicValues left, MqttTopicValues right) => Equals(left, right);

		public static bool operator !=(MqttTopicValues left, MqttTopicValues right) => !Equals(left, right);
	}
}