using System;
using System.Linq;

namespace SmartHomeDotNet.Gpio
{
	public struct GpioId
	{
		/// <summary>
		/// Number of the GPIO pin
		/// </summary>
		public int PinNumber { get; set; }

		/// <inheritdoc />
		public override string ToString()
			=> "gpio_" + PinNumber;
	}
}