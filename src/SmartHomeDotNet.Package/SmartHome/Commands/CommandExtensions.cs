using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Commands
{
	/// <summary>
	/// Extensions on devices to apply the common commands
	/// </summary>
	public static class CommandExtensions
	{
		#region TurnOn
		/// <summary>
		/// Turns on this device
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(), device);

		/// <summary>
		/// Turns on this device to the given brightness
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness), device);

		/// <summary>
		/// Turns on this device to the given color
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, Color color)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(color), device);


		/// <summary>
		/// Turns on this device with a fade in transition
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, TimeSpan transition)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(transition), device);

		/// <summary>
		/// Turns on this device and apply an effect
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, string effect)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(effect), device);

		/// <summary>
		/// Turns on this device to the given brightness with a fade in transition
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, TimeSpan transition)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, transition), device);

		/// <summary>
		/// Turns on this device to the given brightness with a fade in transition and an effect
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, TimeSpan transition, string effect)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, transition, effect), device);

		/// <summary>
		/// Turns on this device to the given brightness and color
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, Color color)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, color), device);

		/// <summary>
		/// Turns on this device to the given brightness and color with a fade in transition
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, Color color, TimeSpan transition)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, color, transition), device);

		/// <summary>
		/// Turns on this device to the given brightness and color, and apply an effect
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, Color color, string effect)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, color, effect), device);

		/// <summary>
		/// Turns on this device to the given brightness and color with a fade in transition and an effect
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOn<T>(this IDevice<T> device, double brightness, Color color, TimeSpan transition, string effect)
			where T : ISupport<TurnOn>
			=> device.Host.Execute(new TurnOn(brightness, color, transition, effect), device);

		#endregion

		#region TurnOff
		/// <summary>
		/// Turns off this device
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOff<T>(this IDevice<T> device)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new TurnOff(), device);

		/// <summary>
		/// Turns off this device with a fade out transition
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation TurnOff<T>(this IDevice<T> device, TimeSpan transition)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new TurnOff {Duration = transition}, device);
		#endregion

		#region Toggle
		/// <summary>
		/// Toggles on/off this device
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation Toggle<T>(this IDevice<T> device)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new Toggle(), device);

		/// <summary>
		/// Toggles on/off this device with a fade transition
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation Toggle<T>(this IDevice<T> device, TimeSpan transition)
			where T : ISupport<TurnOff>
			=> device.Host.Execute(new Toggle {Duration = transition}, device);
		#endregion

		/// <summary>
		/// Send this command to one or more devices
		/// </summary>
		/// <typeparam name="TCommand">The type of the command</typeparam>
		/// <param name="command">The command to send</param>
		/// <param name="devices">The target devices on which this command should be sent</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation SendTo<TCommand>(this TCommand command, params IDevice<ISupport<TCommand>>[] devices)
			where TCommand : ICommand
			=> AsyncContextOperation.WhenAll(devices.GroupBy(d => d.Host).Select(g => g.Key.Execute(command, g)));
		
		/// <summary>
		/// Send this command to one or more devices
		/// </summary>
		/// <typeparam name="TCommand">The type of the command</typeparam>
		/// <param name="command">The command to send</param>
		/// <param name="devices">The target devices on which this command should be sent</param>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation SendTo<TCommand>(this TCommand command, IEnumerable<IDevice<ISupport<TCommand>>> devices)
			where TCommand : ICommand
			=> AsyncContextOperation.WhenAll(devices.GroupBy(d => d.Host).Select(g => g.Key.Execute(command, g)));
	}
}