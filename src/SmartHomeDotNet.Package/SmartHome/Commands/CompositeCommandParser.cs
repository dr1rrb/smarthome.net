using System;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	public sealed class CompositeCommandParser : ICommandParser
	{
		private readonly bool _fallbackOnDefault;
		private ImmutableList<ICommandParser> _parsers;

		public CompositeCommandParser(params ICommandParser[] parsers)
			: this(true, parsers)
		{
		}

		public CompositeCommandParser(bool fallbackOnDefault, params ICommandParser[] parsers)
		{
			_fallbackOnDefault = fallbackOnDefault;
			_parsers = parsers.ToImmutableList();
		}


		public void Register(ICommandParser parser)
			=> ImmutableInterlocked.Update(ref _parsers, (list, @new) => list.Add(@new), parser);

		/// <inheritdoc />
		public bool TryParse(string value, out ICommand command)
		{
			foreach (var parser in _parsers)
			{
				if (parser.TryParse(value, out command))
				{
					return true;
				}
			}

			if (_fallbackOnDefault
				&& CommandParser.Default.TryParse(value, out command))
			{
				return true;
			}

			command = default;
			return false;
		}
	}
}