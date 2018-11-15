using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// A generic <see cref="IDevice"/> which allows type inference
	/// </summary>
	/// <typeparam name="TDevice">Actual type of the device</typeparam>
	public interface IDevice<out TDevice> : IDevice
	{
	}
}