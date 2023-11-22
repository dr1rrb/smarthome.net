#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Mavri.Ha.Api;

internal class CommandId : IDisposable
{
	private static readonly AsyncLocal<CommandId> _current = new();

	public static int Current => (_current.Value ?? Null).Value;

	public static CommandId Null { get; } = new(-1);

	private CommandId(int value)
	{
		Debug.Assert(_current.Value == null || _current.Value == Null, "Cannot stack multiple command ids");

		Value = value;
		_current.Value = this;
	}

	public int Value { get; }

	public void Dispose()
		=> _current.Value = Null;

	public class Provider
	{
		private static int _next;

		public CommandId GetNext()
		{
			return new CommandId(Interlocked.Increment(ref _next));
		}
	}
}