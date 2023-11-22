using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Linq;
using Mavri.Utils;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Gpio
{
	internal abstract class GpioPin : Pin
	{
		public GpioController Controller { get; }

		protected GpioPin(int pinNumber, PinMode mode) : base(pinNumber)
		{
			Controller = BuildController(mode).DisposeWith(Subscriptions);
		}

		private GpioController BuildController(PinMode mode)
		{
			try
			{
				var controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
				
				// Note: We cannot validate the mode before opening the pin using 'controller.IsPinModeSupported'
				//		 This method is supported only for opened pin ...
				controller.OpenPin(PinNumber, mode);

				return controller;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Failed to open pin {PinNumber} in {mode} mode", e);
			}
		}
	}
}