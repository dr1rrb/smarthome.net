using System;
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
		public CommandData(Domain domain, string service, Dictionary<string, object> data, TimeSpan? transition = null)
		{
			Domain = domain;
			Service = service;
			Data = data.ToImmutableDictionary();
			Transition = transition;
		}

		public CommandData(Domain domain, string service, IImmutableDictionary<string, object> data, TimeSpan? transition = null)
		{
			Domain = domain;
			Service = service;
			Data = data;
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
		public IImmutableDictionary<string, object> Data { get; }

		/// <summary>
		/// An optional duration of the effect of the command (cf. remarks)
		/// </summary>
		/// <remarks>This is useful when dimming a light, so the resulting async operation of the API will include this duration and can be easily awaited if needed</remarks>
		public TimeSpan? Transition { get; }
	}
}