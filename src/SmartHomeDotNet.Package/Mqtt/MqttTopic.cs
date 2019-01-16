using System;
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

		public string Topic { get; }

		public string Level { get; }

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

		public MqttTopicValues ToImmutable()
		{
			// For now, we support only top level values
			var values = _children
				.Values
				.ToImmutableDictionary(subTopic => subTopic.Level, subTopic => subTopic._localValue);

			return new MqttTopicValues(Topic, values);
		}
	}
}