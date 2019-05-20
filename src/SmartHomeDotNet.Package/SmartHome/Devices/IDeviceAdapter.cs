using System;
using System.Dynamic;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// An helper interface implemented by the base class <see cref="Device"/> and which is used to ease the creation of devices.
	/// </summary>
	public interface IDeviceAdapter
	{
		/// <summary>
		/// Initialize the device with its values
		/// </summary>
		/// <param name="state">The values of this instance of the device</param>
		void Init(DeviceState state);
	}
}