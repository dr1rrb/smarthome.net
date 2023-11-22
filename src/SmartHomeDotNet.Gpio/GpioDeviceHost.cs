using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using Mavri.Utils;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Gpio
{
	/// <summary>
	/// An <see cref="IDeviceHost"/> for GPIOs devices
	/// </summary>
	internal class GpioDeviceHost : IDeviceHost, IDisposable
	{
		private readonly ImmutableDictionary<int, Pin> _availablePins;

		public GpioDeviceHost(IScheduler scheduler, params Pin[] availablePins)
		{
			Scheduler = scheduler;

			// TODO: Auto build this in Initialize method!
			_availablePins = availablePins.ToImmutableDictionary(p => p.PinNumber);
		}

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public object GetId(object rawId)
			=> GpioId.Parse(rawId);

		/// <inheritdoc />
		public IObservable<DeviceState> GetAndObserveState(IDevice device)
			=> GetPin(device).GetAndObserveState();

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IDevice device)
		{
			var pin = GetPin(device);

			switch (command ?? throw new ArgumentNullException(nameof(command)))
			{
				case TurnOn _ when pin is IOutputPin op:
					return AsyncContextOperation.FromAction(() => op.Set(AsyncContext.CurrentToken, true), $"Turning on pin {pin.PinNumber}");

				case TurnOff _ when pin is IOutputPin op:
					return AsyncContextOperation.FromAction(() => op.Set(AsyncContext.CurrentToken, false), $"Turning off pin {pin.PinNumber}");

				default:
					throw new NotSupportedException($"Command {command.GetType().Name} is not supported (or not supported for device {device.Id}).");
			}
		}

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
		{
			return AsyncContextOperation.WhenAll(devices.Select(dev => Execute(command, dev)));
		}

		private Pin GetPin(IDevice device)
		{
			if (!(device.Id is GpioId id))
			{
				throw new ArgumentOutOfRangeException(nameof(device), $"Device with id '{device.Id}' is not supported by the {nameof(GpioDeviceHost)}.");
			}

			if (!_availablePins.TryGetValue(id.PinNumber, out var pin))
			{
				throw new InvalidOperationException($"Pin #{id.PinNumber} was not configured for this {nameof(GpioDeviceHost)}. You must configure the pin before using it.");
			}

			return pin;
		}

		/// <inheritdoc />
		public void Dispose()
			=> _availablePins.Values.DisposeAllOrLog("Failed to dispose a pin");
	}
}