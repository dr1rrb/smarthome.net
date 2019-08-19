using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="Light"/> device which allows type inference
	/// </summary>
	public interface ILight : IDevice<ILight>, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
	{
	}

	/// <summary>
	/// A device which can interact with the Lights component: <seealso cref="https://www.home-assistant.io/components/light/"/>
	/// </summary>
	public class Light : Device, ILight
	{
		/// <summary>
		/// Gets a boolean which indicates if the light is on or not
		/// </summary>
		public bool IsOn => Raw.state == "on";
	}
}