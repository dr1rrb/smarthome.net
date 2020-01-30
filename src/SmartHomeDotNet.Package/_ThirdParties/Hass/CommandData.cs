﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// The structured data of a command to send to a Home Assistant hub
	/// </summary>
	public struct CommandData
	{
		public CommandData(Domain domain, string service, Dictionary<string, object> parameters, TimeSpan? transition = null)
		{
			Domain = domain;
			Service = service;
			Parameters = parameters.ToImmutableDictionary();
			Transition = transition;
		}

		public CommandData(Domain domain, string service, IImmutableDictionary<string, object> parameters, TimeSpan? transition = null)
		{
			Domain = domain;
			Service = service;
			Parameters = parameters;
			Transition = transition;
		}

		/// <summary>
		/// The target domain on HA
		/// </summary>
		public Domain Domain { get; }

		/// <summary>
		/// The service to invoke on the component
		/// </summary>
		public string Service { get; }

		/// <summary>
		/// The parameters of the service
		/// </summary>
		public IImmutableDictionary<string, object> Parameters { get; }

		/// <summary>
		/// An optional duration of the effect of the command (cf. remarks)
		/// </summary>
		/// <remarks>This is useful when dimming a light, so the resulting async operation of the API will include this duration and can be easily awaited if needed</remarks>
		public TimeSpan? Transition { get; }
	}
}