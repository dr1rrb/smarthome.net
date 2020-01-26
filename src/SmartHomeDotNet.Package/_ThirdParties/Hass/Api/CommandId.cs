using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SmartHomeDotNet.Hass.Api
{
	internal class CommandId : IDisposable
	{
		private static readonly AsyncLocal<CommandId> _current = new AsyncLocal<CommandId>();

		public static int Current => _current.Value.Value;

		public static CommandId Null { get; } = new CommandId(-1);

		private CommandId(int value)
		{
			Debug.Assert(_current.Value == null, "Cannot stack multiple command ids");

			Value = value;
			_current.Value = this;
		}

		public int Value { get; }

		public void Dispose()
			=> _current.Value = null;

		public class Provider
		{
			private static int _next;

			public CommandId GetNext()
			{
				return new CommandId(Interlocked.Increment(ref _next));
			}
		}
	}
}