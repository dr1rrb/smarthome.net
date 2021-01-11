#nullable enable

using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	public interface IDeviceHost : IDeviceActuator
	{
		IObservable<TState> Bind<TState>(object identifier);
	}

	public interface IDeviceHost<in TIdentifier> : IDeviceActuator<TIdentifier>, IDeviceActuator, IUntypedDeviceHost
	{
		IObservable<TState> Bind<TState>(TIdentifier identifier);
	}

	public interface IDeviceHost<in TIdentifier, out TState> : IDeviceActuator<TIdentifier>, IDeviceActuator, IUntypedDeviceHost
	{
		IObservable<TState> Bind(TIdentifier identifier);
	}

	public interface IUntypedDeviceHost : IDeviceActuator
	{
	}
}