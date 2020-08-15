using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using SmartHomeDotNet.Hass.Commands;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	internal static class HomeAssistantCommandExtensions
	{
		public static Dictionary<string, object> ToParameters(this TurnOn on, Domain domain, IEnumerable<IDevice> devices, out TimeSpan? transition)
		{
			transition = null;
			switch (domain)
			{
				case Domain.Light:
					return new Dictionary<string, object>()
						.Add(devices)
						.AddIfValue("brightness", on.Level, level => (int)(level * 255))
						.AddIfValue("rgb_color", on.Color)
						.AddIfValue("transition", on.Duration, out transition)
						.AddIfValue("effect", on.Effect);

				case Domain.InputBoolean:
				case Domain.Switch:
				case Domain.Fan:
					return new Dictionary<string, object>()
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command TurnOn, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this TurnOff off, Domain domain, IEnumerable<IDevice> devices, out TimeSpan? transition)
		{
			transition = null;
			switch (domain)
			{
				case Domain.Light:
					return new Dictionary<string, object>()
						.Add(devices)
						.AddIfValue("transition", off.Duration, out transition);

				case Domain.InputBoolean:
				case Domain.Switch:
				case Domain.Fan:
					return new Dictionary<string, object>()
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command TurnOff, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this Toggle toggle, Domain domain, IEnumerable<IDevice> devices, out TimeSpan? transition)
		{
			transition = null;
			switch (domain)
			{
				case Domain.Light:
					return new Dictionary<string, object>()
						.Add(devices)
						.AddIfValue("transition", toggle.Duration, out transition);

				case Domain.InputBoolean:
				case Domain.Switch:
				case Domain.Fan:
					return new Dictionary<string, object>()
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command Toggle, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this ISelectCommand select, Domain domain, IEnumerable<IDevice> devices)
		{
			var value = select.Value?.ToString() ?? throw new ArgumentNullException(nameof(select.Value), "The selected value cannot be 'null'");

			switch (domain)
			{
				case Domain.InputSelect:
					return new Dictionary<string, object>
						{
							{"option", value}
						}
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command Select, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this SetSpeed setSpeed, Domain domain, IEnumerable<IDevice> devices)
		{
			switch (domain)
			{
				case Domain.Fan:
					return new Dictionary<string, object>
						{
							{"speed", setSpeed.Speed.ToString().ToLowerInvariant()}
						}
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command SetSpeed, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
			}
		}

		public static Dictionary<string, object> ToParameters(this SetText setText, Domain domain, IEnumerable<IDevice> devices)
		{
			switch (domain)
			{
				case Domain.InputText:
					return new Dictionary<string, object>
						{
							{"value", setText.Value}
						}
						.Add(devices);

				default:
					throw new NotSupportedException($"Domain '{domain}' does not support command SetSpeed, but one or more device tries to use it ({devices.Select(d => d.Id.ToString()).JoinBy(", ")})");
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

		public static IImmutableDictionary<string, object> Add(this IImmutableDictionary<string, object> parameters, IEnumerable<IDevice> devices)
		{
			return parameters.Add("entity_id", devices.Select(d => d.Id.ToString()).JoinBy(", "));
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

		public static Dictionary<string, object> AddIfValue(this Dictionary<string, object> parameters, string key, TimeSpan? value, out TimeSpan? transition)
		{
			transition = value;
			return AddIfValue<TimeSpan>(parameters, key, value, d => (int) d.TotalSeconds);
		}

		public static Dictionary<string, object> AddIfValue(this Dictionary<string, object> parameters, string key, Color? value)
			=> AddIfValue<Color>(parameters, key, value, color => new int[] { color.R, color.G, color.B });

		public static Dictionary<string, object> AddIfValue(this Dictionary<string, object> parameters, string key, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				parameters.Add(key, value);
			}

			return parameters;
		}
	}
}