using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	public interface IRGBLight : IDevice<IRGBLight>
	{
	}

	public class RGBLight : DimmableLight, IRGBLight
	{
		public Color Color
		{
			get
			{
				string rgb = Value.rgb_color;

				var values = rgb.Trim('[', ']', ' ').Split(new [] {','}, 3, StringSplitOptions.RemoveEmptyEntries);
				var r = int.Parse(values[0].Trim());
				var g = int.Parse(values[1].Trim());
				var b = int.Parse(values[2].Trim());

				return Color.FromArgb(255, r, g, b);
			}
		}
	}
}
