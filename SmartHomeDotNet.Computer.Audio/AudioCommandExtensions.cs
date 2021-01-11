using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer.Audio
{
	public static class AudioCommandExtensions
	{
		/// <summary>
		/// Sets the volume on the target device
		/// </summary>
		/// <returns>An <see cref="AsyncContextOperation"/>.</returns>
		public static AsyncContextOperation SetVolume<T>(this IDevice<T> device, float volume)
			where T : ISupport<SetVolume>
			=> device.Host.Execute(new SetVolume(volume), device);
	}
}