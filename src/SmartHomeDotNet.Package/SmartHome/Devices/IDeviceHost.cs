using System;
using System.Linq;
using System.Reactive.Concurrency;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// A source of device state changes
	/// </summary>
	public interface IDeviceHost
	{
		/// <summary>
		/// The scheduler to use to handle devices
		/// </summary>
		IScheduler Scheduler { get; }

		/// <summary>
		/// Gets an observable sequence of changes for a device
		/// </summary>
		/// <remarks>It is expected that the sequence starts immediately with an initial of changes to reflect the current state of the device.</remarks>
		/// <param name="id">Identifier of the target device</param>
		/// <returns></returns>
		IObservable<DeviceState> GetAndObserveState(IDevice device);
	}
}