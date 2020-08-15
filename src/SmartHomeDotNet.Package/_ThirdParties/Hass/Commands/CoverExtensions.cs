using System;
using System.Linq;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Commands
{
	public static class CoverExtensions
	{
		public static AsyncContextOperation Open(this IDevice<Cover> cover, TimeSpan? openDuration = null)
			=> cover.Host.Execute(new Open(openDuration), cover);

		public static AsyncContextOperation Close(this IDevice<Cover> cover, TimeSpan? closeDuration = null)
			=> cover.Host.Execute(new Close(closeDuration), cover);

		public static AsyncContextOperation Stop(this IDevice<Cover> cover)
			=> cover.Host.Execute(new Stop(), cover);

		public static AsyncContextOperation SetPosition(this IDevice<Cover> cover, double position)
			=> cover.Host.Execute(new SetPosition(position), cover);
	}
}