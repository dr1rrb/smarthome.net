using System;
using System.Collections.Immutable;
using System.Device.Gpio;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Gpio
{
	internal class GpioInputPin : GpioPin
	{
		public GpioInputPin(int pinNumber) : base(pinNumber, PinMode.Input)
		{
		}

		/// <inheritdoc />
		public override IObservable<DeviceState> GetAndObserveState()
		{
			return Observable
				.Create<bool>(observer =>
				{
					Controller.RegisterCallbackForPinValueChangedEvent(PinNumber, PinEventTypes.Rising | PinEventTypes.Falling, OnPinChanged);
					observer.OnNext((bool)Controller.Read(PinNumber));

					return Disposable.Create(() => Controller.UnregisterCallbackForPinValueChangedEvent(PinNumber, OnPinChanged));

					void OnPinChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
					{
						switch (pinValueChangedEventArgs.ChangeType)
						{
							case PinEventTypes.Rising:
								observer.OnNext(true);
								break;

							case PinEventTypes.Falling:
								observer.OnNext(false);
								break;

							// Ignore case None:
						}
					}
				})
				.Select(isOn => new DeviceState(PinNumber.ToString(), ImmutableDictionary<string, string>.Empty.SetItem("state", isOn ? "on" : "off"), isPersistedState: true));
		}
	}
}