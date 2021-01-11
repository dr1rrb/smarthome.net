#nullable enable

using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	public interface IDeviceState : IDeviceInfo
	{
		/// <summary>
		/// Indicates if this device state is persistent or not (cf. Remarks).
		/// </summary>
		/// <remarks>
		/// A button pressed should nto be flagged as persistent while a temperature update should be.
		/// When a awaiting a device, only persisted state are replayed.
		/// If no persistent state has been stored for the given device, awaiter will have to wait for the next transient state.
		/// </remarks>
		public bool IsPersistent { get; }

		/// <summary>
		/// Gets the status of the current device
		/// </summary>
		public DeviceStatus DeviceStatus { get; }
	}
}