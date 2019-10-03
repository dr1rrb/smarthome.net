using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="RGBLight"/> device which allows type inference
	/// </summary>
	public interface IRGBLight : IDevice<IRGBLight>
	{
	}

	/// <summary>
	/// A light on which color can be customized
	/// </summary>
	public class RGBLight : DimmableLight
	{
		public Color Color
		{
			get
			{
				string rgb = Raw.rgb_color;

				var values = rgb.Trim('[', ']', ' ').Split(new [] {','}, 3, StringSplitOptions.RemoveEmptyEntries);
				var r = int.Parse(values[0].Trim(), CultureInfo.InvariantCulture);
				var g = int.Parse(values[1].Trim(), CultureInfo.InvariantCulture);
				var b = int.Parse(values[2].Trim(), CultureInfo.InvariantCulture);

				return Color.FromArgb(255, r, g, b);
			}
		}
	}
}
