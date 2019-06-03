using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// A generic <see cref="IDevice"/> which allows type inference
	/// </summary>
	/// <remarks>
	/// This interface is designed to allow to write APIs that accepts booth a device,
	/// or its async wrapper <see cref="HomeDevice{TDevice}"/> declared on a home object.
	/// This is useful only for actionable devices.
	/// </remarks>
	/// <typeparam name="TDevice">Actual type of the device</typeparam>
	public interface IDevice<out TDevice> : IDevice
	{
	}
}