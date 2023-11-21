﻿using System;
using System.Collections.Generic;
using System.Text;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Hass.Commands;

public struct Lock : ICommand
{
	public Lock(string? code = null)
	{
		Code = code;
	}

	public string? Code { get; set; }
}