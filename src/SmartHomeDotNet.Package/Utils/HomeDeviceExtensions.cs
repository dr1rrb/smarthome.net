using System;
using System.Linq;
using System.Reactive.Linq;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.Utils
{
	/// <summary>
	/// Extensions over <see cref="HomeDevice{TDevice}"/>
	/// </summary>
	public static class HomeDeviceExtensions
	{
		/// <summary>
		/// Alias of <see cref="ObservableExtensions.WhereUntilChanged{T}"/> specialized for <see cref="HomeDevice{TDevice}"/>
		/// </summary>
		/// <typeparam name="T">Type of the device</typeparam>
		/// <param name="device">The home device</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>An observable sequence which produce a predicate's result changes to true</returns>
		public static IObservable<T> When<T>(this HomeDevice<T> device, Predicate<T> predicate)
			=> device.WhereUntilChanged(predicate);

		/// <summary>
		/// Alias over the <see cref="AsyncContext.Until{TAbortTrigger}"/> specialized for <see cref="HomeDevice{TDevice}"/>
		/// </summary>
		/// <typeparam name="T">Type of the device</typeparam>
		/// <param name="device">The home device</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="assumeInitialState">Determines if the <paramref name="predicate" /> result should be validated or not (cf Remarks)</param>
		/// <remarks>
		/// If the flag <see cref="assumeInitialState"/> is set, all values of the device will be ignored until the predicates returns `true`.
		/// This is useful to filter out initial states until a state is effectively applied.
		/// </remarks>
		/// <returns>An async context that will be cancelled (cf. <see cref="AsyncContext.Cancel"/>) if the <paramref  name="predicate" /> returns `false`.</returns>
		public static AsyncContext While<T>(this HomeDevice<T> device, Predicate<T> predicate, bool assumeInitialState = true)
		{
			var isValid = device.Select(dev => predicate(dev));
			if (assumeInitialState)
			{
				isValid = isValid.SkipWhile(state => !state);
			}

			return AsyncContext.Until(isValid.Where(state => !state));
		}
	}
}