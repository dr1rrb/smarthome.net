using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer
{
	internal class ScreenDevice : IComputerDevice
	{
		private readonly AsyncLock _gate = new AsyncLock();
		private readonly BehaviorSubject<bool> _state = new BehaviorSubject<bool>(true);

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState()
			=> ScreenHelper
				.GetAndObserveState()
				.Select(state => new DeviceState(
					ComputerDeviceId.Screen, 
					ImmutableDictionary<string, string>.Empty.Add("state", state == ScreenHelper.ScreenState.Off ? "off" : "on"), true));

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command)
			=> AsyncContextOperation.StartNew(ct => Execute(command, ct));

		private async Task Execute(ICommand command, CancellationToken ct)
		{
			using (await _gate.LockAsync(ct))
			switch (command)
			{
				case TurnOn on:
				case Toggle toggle when !_state.Value:
					await ScreenHelper.On(ct);
					_state.OnNext(true);
					break;

				case TurnOff off:
				case Toggle toggle:
					await ScreenHelper.Off(ct);
					_state.OnNext(false);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(command), $"Command '{command}' is not supported on Screen");
			}
		}
	}
}