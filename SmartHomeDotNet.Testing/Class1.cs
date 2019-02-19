using System;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Testing
{
	public class TestDeviceHost : IDeviceHost
	{
		private ImmutableDictionary<string, ISubject<DeviceState>> _state = ImmutableDictionary<string, ISubject<DeviceState>>.Empty.WithComparers(StringComparer.InvariantCultureIgnoreCase);

		public TestDeviceHost(IScheduler scheduler = null)
		{
			Scheduler = scheduler ?? TaskPoolScheduler.Default;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void Publish(DeviceState state)
			=> ImmutableInterlocked.GetOrAdd(ref _state, state.DeviceId, new ReplaySubject<DeviceState>(Scheduler)).OnNext(state);

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState(IDevice device)
			=> ImmutableInterlocked.GetOrAdd(ref _state, device.Id, new ReplaySubject<DeviceState>(Scheduler));
	}

	public static class HomeDeviceExtensions
	{
		public static void Set<T>(this HomeDevice<T> device, T state)
		{

		}

		public static void Set<T>(this HomeDevice<T> device, DeviceState state)
		{
			if (device.Host is TestDeviceHost testHost)
			{
				testHost.Publish(state);
			}
			else
			{
				throw new InvalidOperationException("This device was not initialized from a TestDeviceHost, you cannot set its state.");
			}
		}
	}
}
