using System;
using System.Collections.Generic;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Services
{
	/// <summary>
	/// An helper class to use the Fan component (<seealso cref="https://www.home-assistant.io/components/fan/"/>) using a <see cref="HomeAssistantApi"/>.
	/// </summary>
	public static class Fans
	{
		/// <summary>
		/// Turns one or more fans off
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="fans">Fans to turn off</param>
		/// <returns>An <see cref="ApiCall"/>.</returns>
		public static ApiCall TurnOff(
			this HomeAssistantApi ha,
			params IDevice<IFan>[] fans)
			=> ha.Execute("fan", "turn_off", new Dictionary<string, object>
			{
				{"entity_id", fans.Select(l => l.Id).JoinBy(", ")}
			});

		/// <summary>
		/// Turns one or more fans on
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="fans">Fans to turn on</param>
		/// <returns>An <see cref="ApiCall"/>.</returns>
		public static ApiCall TurnOn(
			this HomeAssistantApi ha,
			params IDevice<IFan>[] fans)
			=> ha.Execute("fan", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", fans.Select(l => l.Id).JoinBy(", ")}
			});

		/// <summary>
		/// Sets teh speed of one or more fans
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="fans">Fans to configure</param>
		/// <returns>An <see cref="ApiCall"/>.</returns>
		public static ApiCall SetSpeed(
			this HomeAssistantApi ha,
			Fan.Speeds speed,
			params IDevice<IFan>[] fans)
			=> ha.Execute("fan", "set_speed", new Dictionary<string, object>
			{
				{"entity_id", fans.Select(l => l.Id).JoinBy(", ")},
				{"speed", speed.ToString().ToLowerInvariant()}
			});
	}
}