using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet
{
	public class Room<THome>
		where THome : HomeBase<THome>
	{
		protected static THome Home => HomeBase<THome>.Current;

		protected static HomeDevice<TDevice> Get<TDevice>(object deviceId, [CallerMemberName] string deviceName = null)
			where TDevice : IDeviceAdapter, new()
			=> HomeBase<THome>.Current.GetDefaultDeviceManager(deviceName).GetDevice<TDevice>(deviceId); 
	}
}