using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// Marker interface for an <see cref="IDevice"/> to indicates the supported <see cref="ICommand"/>
	/// </summary>
	/// <typeparam name="T">Type of the command that is supported by this device</typeparam>
	public interface ISupport<T>
		where T : ICommand
	{
	}
}