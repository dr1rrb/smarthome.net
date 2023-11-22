using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// Represents a device of a smart home
	/// </summary>
	public interface IDevice // ==> IDeviceInfo
	{
		/// <summary>
		/// The identifier of this device
		/// </summary>
		object Id { get; }

		/// <summary>
		/// Gets the host which host this device
		/// </summary>
		IDeviceHost Host { get; }
	}
}