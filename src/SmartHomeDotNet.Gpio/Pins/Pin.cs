using System;
using System.Linq;
using System.Reactive.Disposables;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Gpio
{
	internal abstract class Pin : IDisposable
	{
		public int PinNumber { get; }

		public CompositeDisposable Subscriptions { get; } = new CompositeDisposable();

		protected Pin(int pinNumber)
		{
			PinNumber = pinNumber;
		}

		public abstract IObservable<DeviceState> GetAndObserveState();

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Subscriptions.Dispose();
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc />
		~Pin()
		{
			Dispose(false);
		}
	}
}