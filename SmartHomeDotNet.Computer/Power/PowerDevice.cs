using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Mavri.Utils;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer
{
	internal class PowerDevice : IComputerDevice
	{
		private readonly AsyncLock _gate = new AsyncLock();
		private readonly BehaviorSubject<bool> _state = new BehaviorSubject<bool>(true);

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState()
			=>  Observable
				.Never<DeviceState>()
				.StartWith(new DeviceState(
					ComputerDeviceId.Power,
					ImmutableDictionary<string, string>.Empty.Add("state", "on"),
					true));

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command)
			=> AsyncContextOperation.StartNew(ct => Execute(command, ct));

		private async Task Execute(ICommand command, CancellationToken ct)
		{
			using var _ = await _gate.LockAsync(ct);
			switch (command)
			{
				case Sleep sleep:
					_state.OnNext(false);
					await PowerHelper.Sleep(ct);
					break;

				case TurnOff off:
					_state.OnNext(false);
					await PowerHelper.Off(ct);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(command), $"Command '{command}' is not supported on Power");
			}
		}
	}
}