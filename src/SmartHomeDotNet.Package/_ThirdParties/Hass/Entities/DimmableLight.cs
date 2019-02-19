using System;
using System.Globalization;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="DimmableLight"/> device which allows type inference
	/// </summary>
	public interface IDimmableLight : IDevice<IDimmableLight>
	{
	}

	/// <summary>
	/// a light on which brightness can be customized
	/// </summary>
	public class DimmableLight : Light, IDimmableLight
	{
		/// <summary>
		/// Gets teh current brightness of the light
		/// </summary>
		public double Brightness => double.Parse(Raw.brightness, CultureInfo.InvariantCulture) / 255.0; // We parse double as the value might be 0.0
	}
}