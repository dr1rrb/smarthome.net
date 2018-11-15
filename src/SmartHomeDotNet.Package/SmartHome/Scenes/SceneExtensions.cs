using System;
using System.Linq;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// Extensions over <see cref="Scene"/> and <see cref="ISceneHost"/>.
	/// </summary>
	public static class SceneExtensions
	{
		public static IDisposable Subscribe<T>(this IObservable<T> source, ISceneHost host, TimeSpan? retryDelay = null)
		{
			return source
				.Retry(retryDelay ?? TimeSpan.FromSeconds(10), host.Scheduler)
				.Subscribe();
		}
	}
}