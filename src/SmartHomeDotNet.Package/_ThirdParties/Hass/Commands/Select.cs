using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands
{
	internal interface ISelectCommand : ICommand
	{
		object Value { get; }
	}

	public struct Select<T> : ISelectCommand, ICommand
	{
		public Select(T value)
		{
			Value = value;
		}

		public T Value { get; }

		object ISelectCommand.Value => Value;
	}
}
