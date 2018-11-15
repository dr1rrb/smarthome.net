using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SmartHomeDotNet.Logging
{
	public sealed class ConsoleLogger : ILogger
	{
		public static ILogger Instance { get; } = new ConsoleLogger();

		private ConsoleLogger() { }

		public void Debug(string message) => Write(message);
		public void Info(string message) => Write(message);
		public void Warning(string message) => Write(message);
		public void Error(string message, Exception ex = null) => Write(message, ex);

		private void Write(string message, Exception ex = null, [CallerMemberName] string level = null)
			=> Console.WriteLine($"{DateTime.Now:G} [{level.ToUpperInvariant()}] {message} {ex}");
	}
}