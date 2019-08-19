using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Services
{
	/// <summary>
	/// An helper class to use the inputs components using a <see cref="HomeAssistantApi"/>.
	/// </summary>
	public static class Inputs
	{
		/// <summary>
		/// Turns off one or more boolean inputs
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="inputs">The inputs to turn off</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOff(
			this HomeAssistantApi ha,
			params IDevice<IInputBoolean>[] inputs)
			=> ha.Execute("input_boolean", "turn_off", new Dictionary<string, object>
			{
				{"entity_id", inputs.Select(l => l.Id).JoinBy(", ")}
			});

		/// <summary>
		/// Turns on one or more boolean inputs
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="inputs">The inputs to turn on</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn(
			this HomeAssistantApi ha,
			params IDevice<IInputBoolean>[] inputs)
			=> ha.Execute("input_boolean", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", inputs.Select(l => l.Id).JoinBy(", ")}
			});

		/// <summary>
		/// Toggles one or more boolean inputs
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="inputs">The inputs to toggle</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation Toggle(
			this HomeAssistantApi ha,
			params IDevice<IInputBoolean>[] inputs)
			=> ha.Execute("input_boolean", "toggle", new Dictionary<string, object>
			{
				{"entity_id", inputs.Select(l => l.Id).JoinBy(", ")}
			});

		/// <summary>
		/// Sets the selected value one or more select inputs
		/// </summary>
		/// <param name="ha">The HA API</param>
		/// <param name="inputs">The select inputs to set</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation Select<T>(
			this HomeAssistantApi ha,
			T option,
			params IDevice<IInputSelect<T>>[] inputs)
			=> ha.Execute("input_select", "select_option", new Dictionary<string, object>
			{
				{"entity_id", inputs.Select(l => l.Id).JoinBy(", ")},
				{"option", option.ToString()}
			});

		//public static HomeAssistantApi.Call Set(
		//	this HomeAssistantApi ha,
		//	TimeSpan time,
		//	params IDevice<IInputTimespan>[] inputs)
		//	=> ha.Execute("input_boolean", "set_datetime", new Dictionary<string, object>
		//	{
		//		{"entity_id", inputs.Select(l => l.Id).JoinBy(", ")}
		//	});
	}
}
