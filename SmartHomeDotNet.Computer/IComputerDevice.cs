using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer
{
	internal interface IComputerDevice
	{
		IObservable<DeviceState> GetAndObserveState();

		AsyncContextOperation Execute(ICommand command);
	}
}