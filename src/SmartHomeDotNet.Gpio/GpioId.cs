using System;
using System.Linq;

namespace SmartHomeDotNet.Gpio
{
	public struct GpioId
	{
		public GpioId(int pinNumber)
		{
			PinNumber = pinNumber;
		}

		/// <summary>
		/// Number of the GPIO pin
		/// </summary>
		public int PinNumber { get; set; }

		/// <inheritdoc />
		public override string ToString()
			=> "gpio_" + PinNumber;

		public static GpioId Parse(object rawId)
		{
			if (rawId is GpioId id)
			{
				return id;
			}

			if (rawId == null)
			{
				throw new ArgumentNullException(nameof(rawId), "The ID is null");
			}

			if (!(rawId is int pinNumber))
			{
				throw new ArgumentOutOfRangeException(nameof(rawId), "The value is not an integer");
			}

			return new GpioId(pinNumber);
		}
	}
}