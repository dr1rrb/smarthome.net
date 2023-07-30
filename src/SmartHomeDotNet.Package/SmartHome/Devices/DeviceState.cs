using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// A set of properties for a device
	/// </summary>
	public class DeviceState
	{
		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="deviceId">Id of the device</param>
		/// <param name="properties">The properties of the device</param>
		/// <param name="isPersistedState">A boolean which indicates if this device state is a transient state (e.g. button pressed), or not</param>
		public DeviceState(object deviceId, ImmutableDictionary<string, string> properties, bool isPersistedState)
		{
			DeviceId = deviceId;
			Properties = properties; // TODO .WithComparers(StringComparer.OrdinalIgnoreCase); => Actually we should even allow snake casing vs camel case
			IsPersistedState = isPersistedState;
		}

		/// <summary>
		/// Id of the updated device
		/// </summary>
		public object DeviceId { get; }

		/// <summary>
		/// Set of all properties of this device
		/// </summary>
		public ImmutableDictionary<string, string> Properties { get; }

		/// <summary>
		/// Get a boolean which indicates if this device state is a transient state (e.g. button pressed), or not.
		/// </summary>
		public bool IsPersistedState { get; }

		internal ExpandoObject ToDynamic()
		{
			// Clone it to an expando object
			var device = new ExpandoObject();
			var deviceValues = device as IDictionary<string, object>;
			foreach (var property in Properties)
			{
				deviceValues[property.Key] = property.Value;
			}

			return device;
		}
	}
}