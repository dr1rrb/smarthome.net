using System;
using System.Collections.Immutable;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	public class LockAdapter : ICommandAdapter
	{
		/// <inheritdoc />
		public bool TryGetData(Component component, ICommand command, out CommandData data)
		{
			switch (component)
			{
				case "lock":
					var p = ImmutableDictionary<string, object>.Empty;
					switch (command)
					{
						case Lock @lock:
							if (@lock.Code.HasValue())
							{
								p = p.Add("code", @lock.Code);
							}
							data = new CommandData(Domain.Lock, "lock", p);
							return true;

						case Unlock unlock:
							if (unlock.Code.HasValue())
							{
								p = p.Add("code", unlock.Code);
							}
							data = new CommandData(Domain.Lock, "unlock", p);
							return true;
					}
					break;
			}

			data = default;
			return false;
		}
	}
}