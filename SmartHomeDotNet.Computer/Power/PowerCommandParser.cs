using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Computer
{
	public class PowerCommandParser : ICommandParser
	{
		/// <inheritdoc />
		public bool TryParse(string value, out ICommand command)
		{
			switch (value.ToLowerInvariant())
			{
				case "sleep":
					command = new Sleep();
					return true;

				case "off":
					command = new TurnOff();
					return true;
			}

			command = default;
			return false;
		}
	}
}