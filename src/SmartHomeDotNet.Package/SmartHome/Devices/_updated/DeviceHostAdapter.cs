#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	internal class DeviceHostAdapter<TIdentifier> : IDeviceHost
	{
		private readonly IDeviceHost<TIdentifier> _inner;

		public DeviceHostAdapter(IDeviceHost<TIdentifier> inner)
			=> _inner = inner;

		public IObservable<TState> Bind<TState>(object identifier)
			=> _inner.Bind<TState>((TIdentifier)identifier);

		public AsyncContextOperation Execute(ICommand command, params object[] devices)
			=> _inner.Execute(command, devices.Cast<TIdentifier>().ToArray());
	}

	internal class DeviceHostAdapter<TIdentifier, TState> : IDeviceHost
	{
		private readonly IDeviceHost<TIdentifier, TState> _inner;

		public DeviceHostAdapter(IDeviceHost<TIdentifier, TState> inner)
			=> _inner = inner;

		public IObservable<TState> Bind<TState>(object identifier)
			=> (IObservable<TState>)(object)_inner.Bind((TIdentifier)identifier);

		public AsyncContextOperation Execute(ICommand command, params object[] devices)
			=> _inner.Execute(command, devices.Cast<TIdentifier>().ToArray());
	}
	
	internal class UntypedDeviceHostAdapter : IDeviceHost
	{
		private readonly IUntypedDeviceHost _inner;
		private readonly (MethodInfo method, Type[] typeArgs)[] _binds;

		public UntypedDeviceHostAdapter(IUntypedDeviceHost inner)
		{
			_inner = inner ?? throw new ArgumentNullException(nameof(inner), "Host must not be null.");
			_binds = inner
				.GetType()
				.GetInterfaces()
				.Where(i => i.Name.StartsWith(nameof(IDeviceHost), StringComparison.Ordinal))
				.Select(i => i.GetMethod(nameof(IDeviceHost.Bind)))
				.Select(bind => (method: bind, typeArgs: bind.GetGenericArguments()))
				.OrderBy(bind => bind.typeArgs.Length) // We try the most specific version first
				.ToArray();

			if (_binds.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(inner), "The host must have a Bind method.");
			}
		}

		public IObservable<TState> Bind<TState>(object identifier)
		{
			//var idType = identifier.GetType();
			var stType = typeof(TState);

			foreach (var bind in _binds)
			{
				switch (bind.typeArgs.Length)
				{
					case 1 when bind.typeArgs[0] == stType:
						return (IObservable<TState>) bind.method.Invoke(_inner, new[] {identifier});

					case 0:
						return (IObservable<TState>)bind.method.Invoke(_inner, new[] { identifier });
				}
			}

			return (IObservable<TState>)_binds.Last().method.Invoke(_inner, new[] { identifier });
		}

		public AsyncContextOperation Execute(ICommand command, params object[] devices)
			=> _inner.Execute(command, devices);
	}
}