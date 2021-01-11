#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace SmartHomeDotNet.SmartHome.Devices_2
{
	public class DeviceRegistry
	{
		///// <summary>
		///// Discovers hosts using reflection
		///// </summary>
		///// <returns></returns>
		//public static IEnumerable<IDeviceHost> DiscoverHosts()
		//{

		//}

		private static readonly AsyncLocal<DeviceRegistry?> _current = new AsyncLocal<DeviceRegistry?>();
		private readonly List<IDeviceHost> _hosts = new List<IDeviceHost>();
		private readonly IScheduler _scheduler;

		private ImmutableDictionary<object, object> _devices = ImmutableDictionary<object, object>.Empty;

		public static DeviceRegistry Current => _current.Value ?? throw new InvalidOperationException("No current device registry.");

		public DeviceRegistry(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void RegisterHost(IDeviceHost host)
			=> _hosts.Add(host);

		//public void RegisterHost<TIdentifier>(IDeviceHost<TIdentifier> host)
		//	=> _hosts.Add(new DeviceHostAdapter<TIdentifier>(host));

		//public void RegisterHost<TIdentifier, TState>(IDeviceHost<TIdentifier, TState> host)
		//	=> _hosts.Add(new DeviceHostAdapter<TIdentifier, TState>(host));

		public void RegisterHost(IUntypedDeviceHost host)
			=> _hosts.Add(new UntypedDeviceHostAdapter(host));

		public IDisposable SetAsCurrent()
		{
			if (_current.Value is { })
			{
				throw new InvalidOperationException("Cannot stack multiple device registry at once");
			}

			_current.Value = this;

			return Disposable.Create(() =>
			{
				var current = _current.Value;
				_current.Value = null;

				if (current != this)
				{
					throw new InvalidOperationException("Cannot stack multiple device registry at once");
				}
			});
		}

		public Dev<TState> Bind<TState>(object identifier)
			where TState : IDeviceState
			=> (Dev<TState>)ImmutableInterlocked.GetOrAdd(ref _devices, identifier, Create<TState>);

		private Dev<TState> Create<TState>(object identifier)
			where TState : IDeviceState
		{
			foreach (var host in _hosts)
			{
				if (host.Bind<TState>(identifier) is { } source)
				{
					return new Dev<TState>(host, identifier, source, _scheduler);
				}
			}

			throw new KeyNotFoundException($"Failed to bind to device {identifier}");
		}
	}
}