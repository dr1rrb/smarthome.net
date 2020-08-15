using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands
{
	public struct Lock : ICommand
	{
		public Lock(string code)
		{
			Code = code;
		}

		public string Code { get; set; }
	}

	public struct Unlock : ICommand
	{
		public Unlock(string code)
		{
			Code = code;
		}

		public string Code { get; set; }
	}
}
