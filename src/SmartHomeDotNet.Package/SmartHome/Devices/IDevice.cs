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
		object Id { get; }

		/// <summary>
		/// Gets the host which host this device
		/// </summary>
		IDeviceHost Host { get; }
	}

	public interface ILazyDevice : IDevice
	{
		void TryInit(object id, IDeviceHost host);
	}
}