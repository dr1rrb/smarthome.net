using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	internal static class HomeAssistantCommandExtensions
	{
		public static Dictionary<string, object> ToParameters(this TurnOn on, Component component, IEnumerable<IDevice> deviceId)
		{
			switch (component)
			{
				case Component.Light:
					return new Dictionary<string, object>()
						.Add(deviceId)
						.AddIfValue("brightness", on.Level, level => (int)(level * 255))
						.AddIfValue("rgb_color", on.Color)
						.AddIfValue("transition", on.Duration);

				case Component.Switch:
					return new Dictionary<string, object>()
						.Add(deviceId);

				default:
					throw new NotSupportedException($"Component '{component}' does not support command TurnOn, but on or more device tries to use it ({deviceId.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this TurnOff off, Component component, IEnumerable<IDevice> deviceId)
		{
			switch (component)
			{
				case Component.Light:
					return new Dictionary<string, object>()
						.Add(deviceId)
						.AddIfValue("transition", off.Duration);

				case Component.Switch:
					return new Dictionary<string, object>()
						.Add(deviceId);

				default:
					throw new NotSupportedException($"Component '{component}' does not support command TurnOff, but on or more device tries to use it ({deviceId.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this Toggle toggle, Component component, IEnumerable<IDevice> deviceId)
		{
			switch (component)
			{
				case Component.Light:
					return new Dictionary<string, object>()
						.Add(deviceId)
						.AddIfValue("transition", toggle.Duration);

				case Component.Switch:
					return new Dictionary<string, object>()
						.Add(deviceId);

				default:
					throw new NotSupportedException($"Component '{component}' does not support command Toggle, but on or more device tries to use it ({deviceId.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> Add(this Dictionary<string, object> parameters, EntityId id)
		{
			parameters.Add("entity_id", id.ToString());

			return parameters;
		}

		public static Dictionary<string, object> Add(this Dictionary<string, object> parameters, IEnumerable<IDevice> devices)
		{
			parameters.Add("entity_id", devices.Select(d => d.Id.ToString()).JoinBy(", "));

			return parameters;
		}

		public static Dictionary<string, object> AddIfValue<T>(this Dictionary<string, object> parameters, string key, T? value, Func<T, object> format = null)
			where T : struct
		{
			if (value.HasValue)
			{
				format = format ?? (v => v.ToString());
				parameters.Add(key, format(value.Value));
			}

			return parameters;
		}

		public static Dictionary<string, object> AddIfValue(this Dictionary<string, object> parameters, string key, TimeSpan? value)
			=> AddIfValue<TimeSpan>(parameters, key, value, d => (int) d.TotalSeconds);

		public static Dictionary<string, object> AddIfValue(this Dictionary<string, object> parameters, string key, Color? value)
			=> AddIfValue<Color>(parameters, key, value, color => new int[] { color.R, color.G, color.B });
	}
}