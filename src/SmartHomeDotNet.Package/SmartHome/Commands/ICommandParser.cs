using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// A parser capable to create an instance of an <see cref="ICommand"/> from its string representation
	/// </summary>
	public interface ICommandParser
	{
		/// <summary>
		/// Try to parse a command from it string representation
		/// </summary>
		/// <param name="value">The source string to parse</param>
		/// <param name="command">The result command</param>
		/// <returns>A bool which indicates if the parsing was successful or not</returns>
		bool TryParse(string value, out ICommand command);
	}
}