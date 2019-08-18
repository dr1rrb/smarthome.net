using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;

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
		/// Parse and validate the configured raw ID into the expected ID type for the devices managed by this host
		/// </summary>
		/// <param name="rawId">The raw ID configured by the end user, usually a string or a int</param>
		/// <returns>The structured device ID</returns>
		object GetId(object rawId);

		/// <summary>
		/// Gets an observable sequence of changes for a device
		/// </summary>
		/// <remarks>It is expected that the sequence starts immediately with an initial of changes to reflect the current state of the device.</remarks>
		/// <param name="device">Identifier of the target device</param>
		/// <returns></returns>
		IObservable<DeviceState> GetAndObserveState(IDevice device);

		/// <summary>
		/// Executes a command on the target device
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="device">The device on which the command have to be executed</param>
		/// <returns>An async operation</returns>
		AsyncContextOperation Execute(ICommand command, IDevice device);

		/// <summary>
		/// Executes a command on the target device
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="devices">The devices on which the command have to be executed</param>
		/// <returns>An async operation</returns>
		AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices);
	}
}