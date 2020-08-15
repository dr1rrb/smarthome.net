using System;
using System.Collections.Immutable;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands
{
	public class CoverAdapter : ICommandAdapter
	{
		/// <inheritdoc />
		public bool TryGetData(Component domain, ICommand command, out CommandData data)
		{
			if (domain == "cover")
			{
				switch (command)
				{
					case Close close:
						data = new CommandData("cover", "close_cover", ImmutableDictionary<string, object>.Empty, close.Duration);
						return true;

					case Open open:
						data = new CommandData("cover", "open_cover", ImmutableDictionary<string, object>.Empty, open.Duration);
						return true;

					case Stop _:
						data = new CommandData("cover", "stop_cover", ImmutableDictionary<string, object>.Empty);
						return true;

					case SetPosition set:
						data = new CommandData("cover", "set_cover_position", ImmutableDictionary<string, object>.Empty.Add("position", Math.Max(0, Math.Min(100, (int)(set.Position * 100)))));
						return true;
				}
			}

			data = default;
			return false;
		}
	}
}