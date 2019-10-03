using System;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands
{
	/// <summary>
	/// Command to set text on an <see cref="InputText"/>
	/// </summary>
	public struct SetText : ICommand
	{
		/// <summary>
		/// The value to set
		/// </summary>
		public string Value { get; set; }
	}
}