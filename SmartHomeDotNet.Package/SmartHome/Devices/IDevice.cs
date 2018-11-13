using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// Represents a device of a smart home
	/// </summary>
	public interface IDevice
	{
		/// <summary>
		/// The identifier of this device
		/// </summary>
		string Id { get; }
	}
}