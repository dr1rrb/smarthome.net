using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Subjects;

namespace SmartHomeDotNet.Mqtt
{
	internal sealed class MqttTopic
	{
		private ImmutableDictionary<string, MqttTopic> _children = ImmutableDictionary<string, MqttTopic>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
		private bool _hasLocalValue;
		private string _localValue;

		/// <summary>
		/// The full topic path
		/// </summary>
		public string Topic { get; }

		/// <summary>
		/// The last level of the topic
		/// </summary>
		public string Level { get; }

		/// <summary>
		/// The parent topic, or null if this topic is root
		/// </summary>
		public MqttTopic Parent { get; }

		/// <summary>
		/// Creates a root topic
		/// </summary>
		public MqttTopic()
		{
		}

		private MqttTopic(MqttTopic parent, string level)
		{
			Level = level;
			Parent = parent;
			Topic = parent.Topic == null //i.e. Root
				? level
				: parent.Topic + "/" + level;
		}

		public bool HasValue => _hasLocalValue || _children.Any(c => c.Value.HasValue);

		public IObservable<(MqttTopic topic, string value)> ObserveUpdates() => _updates;
		private readonly Subject<(MqttTopic topic, string value)> _updates = new Subject<(MqttTopic, string)>();
		private void ChildUpdated((MqttTopic topic, string value) change)
		{
			_updates.OnNext(change);
			Parent?.ChildUpdated(change);
		}

		public MqttTopic GetChild(string level) => ImmutableInterlocked.GetOrAdd(ref _children, level, n => new MqttTopic(this, n));

		public bool TryUpdate(string value, bool retained)
		{
			if (!retained && !_hasLocalValue)
			{
				_updates.OnNext((this, value));
				Parent?.ChildUpdated((this, value));

				return true;
			}
			else if (_hasLocalValue && (_localValue?.Equals(value, StringComparison.InvariantCultureIgnoreCase) ?? value == null))
			{
				return false;
			}
			else
			{
				_localValue = value;
				_hasLocalValue = true;

				_updates.OnNext((this, value));
				Parent?.ChildUpdated((this, value));

				return true;
			}
		}

		public bool IsAncestor(MqttTopic topic)
		{
			while (topic != null)
			{
				if (topic == this)
				{
					return true;
				}

				topic = topic.Parent;
			}

			return false;
		}

		public MqttTopicValues ToImmutable(MqttTopic changedTopic, string changedValue)
		{
			// For now, we support only top level values

			var localValue = _localValue;
			var childrenValues = _children
				.Values
				.Where(child => child._hasLocalValue)
				.Select(child => (child, child._localValue)) as IEnumerable<(MqttTopic topic, string value)>;

			if (changedTopic == this)
			{
				localValue = changedValue;
			}
			else if (changedTopic?.Parent == this)
			{
				childrenValues = childrenValues
					.Where(child => child.topic != changedTopic)
					.Concat(new [] {(changedTopic, changedValue)});
			}

			return new MqttTopicValues(
				Topic, 
				localValue, 
				childrenValues.ToImmutableDictionary(c => c.topic.Level, c => c.value),
				changedTopic == null
					? HasValue // For the initial value we consider as retained only if we actually have any values
					: changedTopic._localValue == changedValue // For changes notif, we only have to check if the value was effectively persisted or not
				);
		}
	}
}