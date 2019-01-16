﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Services
{
	public static class Lights
	{
		public static HomeAssistantApi.Call TurnOff(
			this HomeAssistantApi ha,
			params IDevice<ILight>[] lights)
			=> ha.Execute("light", "turn_off", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")}
			});

		public static HomeAssistantApi.Call TurnOff(
			this HomeAssistantApi ha,
			TimeSpan transition,
			params IDevice<ILight>[] lights)
			=> ha.Execute("light", "turn_off", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")},
				{"transition", (int) transition.TotalSeconds}
			});

		public static HomeAssistantApi.Call TurnOn(this HomeAssistantApi ha, params IDevice<ILight>[] lights)
			=> ha.Execute("light", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")}
			});

		public static HomeAssistantApi.Call TurnOn(
			this HomeAssistantApi ha,
			double brightness,
			params IDevice<IDimmableLight>[] lights)
			=> ha.Execute("light", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")},
				{"brightness", (int) (brightness * 255)},
			});

		public static HomeAssistantApi.Call TurnOn(
			this HomeAssistantApi ha,
			double brightness,
			TimeSpan transition,
			params IDevice<IDimmableLight>[] lights)
			=> ha.Execute("light", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")},
				{"brightness", (int) (brightness * 255)},
				{"transition", (int) transition.TotalSeconds}
			});

		public static HomeAssistantApi.Call TurnOn(
			this HomeAssistantApi ha,
			double brightness,
			Color color,
			params IDevice<IRGBLight>[] lights)
			=> ha.Execute("light", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")},
				{"brightness", (int) (brightness * 255)},
				{"rgb_color", new int[] {color.R, color.G, color.B}}
			});

		public static HomeAssistantApi.Call TurnOn(
			this HomeAssistantApi ha,
			double brightness,
			Color color,
			TimeSpan transition,
			params IDevice<IRGBLight>[] lights)
			=> ha.Execute("light", "turn_on", new Dictionary<string, object>
			{
				{"entity_id", lights.Select(l => l.Id).JoinBy(", ")},
				{"brightness", (int) (brightness * 255)},
				{"rgb_color", new int[] {color.R, color.G, color.B}},
				{"transition", (int) transition.TotalSeconds}
			});
	}
}