using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// A default parser for common commands
	/// </summary>
	public sealed class CommandParser : ICommandParser
	{
		/// <summary>
		/// Singleton instance of the parser
		/// </summary>
		public static ICommandParser Default { get; } = new CommandParser();

		private CommandParser()
		{
		}

		/// <inheritdoc />
		public bool TryParse(string value, out ICommand command)
		{
			switch (value.ToLowerInvariant())
			{
				case "on":
					command = new TurnOn();
					return true;

				case "off":
					command = new TurnOff();
					return true;

				default:
					command = default;
					return false;
			}
		}
	}
}