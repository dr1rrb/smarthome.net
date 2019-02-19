using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Hass.Entities
{
	/// <summary>
	/// A marker interface for <see cref="InputSelect{T}"/> device which allows type inference
	/// </summary>
	public interface IInputSelect<T> : IDevice<IInputSelect<T>>
	{
	}

	/// <summary>
	/// A device for <seealso cref="https://www.home-assistant.io/components/input_select/"/>
	/// </summary>
	/// <typeparam name="T">Type of the selectable value</typeparam>
	public class InputSelect<T> : Device, IInputSelect<T>
		where T : struct
	{
		/// <summary>
		/// Gets the currently selected value
		/// </summary>
		public T Value => (T)Enum.Parse(typeof(T), Raw.state, ignoreCase: true);

		public static implicit operator T(InputSelect<T> input)
			=> input.Value;
	}
}