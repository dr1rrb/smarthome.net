using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands;

public struct Unlock : ICommand
{
	public Unlock(string? code = null)
	{
		Code = code;
	}

	public string? Code { get; set; }
}