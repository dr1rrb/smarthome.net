using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	public static class CommandParserExtensions
	{
		public static ICommand ParseOrDefault(this ICommandParser parser, string command)
			=> parser.TryParse(command, out var c) ? c : default;
	}
}