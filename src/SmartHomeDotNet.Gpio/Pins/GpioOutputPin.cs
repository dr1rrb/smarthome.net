using System;
using System.Collections.Immutable;
using System.Device.Gpio;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Mavri.Utils;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Gpio
{
	internal interface IOutputPin
	{
		Task Set(CancellationToken ct, bool state);
	}

	internal sealed class GpioOutputPin : GpioPin, IOutputPin
	{
		private readonly BehaviorSubject<bool> _state;

		public GpioOutputPin(int pinNumber, bool initialValue) : base(pinNumber, PinMode.Output)
		{
			_state = new BehaviorSubject<bool>(initialValue).DisposeWith(Subscriptions);

			// Ensure initial state
			Controller.Write(PinNumber, initialValue);
		}

		/// <inheritdoc />
		public override IObservable<DeviceState> GetAndObserveState()
			=> _state
				.DistinctUntilChanged()
				.Select(isOn => new DeviceState(
					PinNumber.ToString(), 
					ImmutableDictionary<string, string>.Empty.SetItem("state", isOn ? "on" : "off"), 
					isPersistedState: true));

		/// <summary>
		/// Sets the current value of the pin
		/// </summary>
		/// <param name="state">The value to write to this GPIO pin</param>
		public async Task Set(CancellationToken ct, bool state)
		{
			Controller.Write(PinNumber, state);
			_state.OnNext(state);
		}
	}
}