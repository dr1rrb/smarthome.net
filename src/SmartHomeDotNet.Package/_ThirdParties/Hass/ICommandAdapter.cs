using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// An adapter which adapts a generic <see cref="ICommand"/> to a <see cref="CommandData"/> which can be sent to a Home Assistant hub
	/// </summary>
	public interface ICommandAdapter
	{
		/// <summary>
		/// Attempt to convert a generic <see cref="ICommand"/> to a <see cref="CommandData"/>
		/// </summary>
		/// <param name="domain">The domain of the entities to which the command is going to be sent</param>
		/// <param name="command">The comment to adapt</param>
		/// <param name="data">The structured data that is going to be sent to HA</param>
		/// <returns>A boolean which indicates if this adapter is able to generate a valid data or not.</returns>
		bool TryGetData(Component domain, ICommand command, out CommandData data);
	}
}