using System;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// A set of properties for a device
	/// </summary>
	public class DeviceState
	{
		public DeviceState(string deviceId, ImmutableDictionary<string, string> properties)
		{
			DeviceId = deviceId;
			Properties = properties;
		}

		/// <summary>
		/// Id of the updated device
		/// </summary>
		public string DeviceId { get; }

		/// <summary>
		/// Set of all properties of this device
		/// </summary>
		public ImmutableDictionary<string, string> Properties { get; }
	}
}