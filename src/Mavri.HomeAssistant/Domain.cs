using System;
using System.Linq;

namespace Mavri.Ha
{
	/// <summary>
	/// Enumeration of the common devices components of home assistant
	/// </summary>
	public struct Domain
	{
		// Note: they are exposed as **const** string in order to be able to use them in a switch cases

		public const string Switch = "switch";

		public const string Light = "light";

		public const string Fan = "fan";

		public const string InputBoolean = "input_boolean";

		public const string InputSelect = "input_select";

		public const string InputText = "input_text";

		public const string Lock = "lock";

		/// <summary>
		/// The name of this component
		/// </summary>
		public string Name { get; }

		public Domain(string name)
		{
			Name = name.ToLowerInvariant();
		}

		/// <inheritdoc />
		public override string ToString() => Name;

		public static implicit operator string(Domain component) => component.Name;

		public static implicit operator Domain(string name) => new Domain(name);
	}
}