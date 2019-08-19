using System;
using System.Linq;
using System.Net.Sockets;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// Enumeration of the common devices components of home assistant
	/// </summary>
	public struct Component
	{
		// Note: they are exposed as **const** string in order to be able to use them in a switch cases

		public const string Switch = "switch";

		public const string Light = "light";

		public const string Fan = "fan";

		public const string Sensor = "sensor";

		public const string InputBoolean = "input_boolean";

		public const string InputSelect = "input_select";

		/// <summary>
		/// The name of this component
		/// </summary>
		public string Name { get; }

		public Component(string name)
		{
			Name = name.ToLowerInvariant();
		}

		/// <inheritdoc />
		public override string ToString() => Name;

		public static implicit operator string(Component component) => component.Name;

		public static implicit operator Component(string name) => new Component(name);
	}
}