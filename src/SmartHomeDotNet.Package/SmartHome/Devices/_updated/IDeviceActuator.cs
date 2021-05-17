#nullable enable

using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	public interface IDeviceActuator
	{
		/// <summary>
		/// Executes a command on the target device
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="devices">The devices on which the command have to be executed</param>
		/// <returns>An async operation</returns>
		AsyncContextOperation Execute(ICommand command, params object[] devices);
	}

	public interface IDeviceActuator<in TIdentifier> : IDeviceActuator
	{
		/// <summary>
		/// Executes a command on the target device
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="devices">The devices on which the command have to be executed</param>
		/// <returns>An async operation</returns>
		AsyncContextOperation Execute(ICommand command, params TIdentifier[] devices);

#if NETSTANDARD2_1
		AsyncContextOperation IDeviceActuator.Execute(ICommand command, params object[] devices)
			=> Execute(command, devices.Cast<TIdentifier>().ToArray());
#endif
	}
}