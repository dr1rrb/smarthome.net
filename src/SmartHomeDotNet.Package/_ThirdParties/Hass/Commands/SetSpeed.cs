using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands
{
	public struct SetSpeed : ICommand
	{
		public SetSpeed(Fan.Speeds speed)
		{
			Speed = speed;
		}

		public Fan.Speeds Speed { get; }
	}
}
