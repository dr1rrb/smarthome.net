using System;
using System.Linq;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// Extensions over <see cref="Automation"/> and <see cref="IAutomationHost"/>.
	/// </summary>
	public static class AutomationExtensions
	{
		public static IDisposable Subscribe<T>(this IObservable<T> source, IAutomationHost host, TimeSpan? retryDelay = null)
		{
			return source
				.Retry(retryDelay ?? TimeSpan.FromSeconds(10), host.Scheduler)
				.Subscribe();
		}

		public static IDisposable Subscribe<T>(this IObservable<T> source, Automation automation, TimeSpan? retryDelay = null)
		{
			return source
				.Retry(retryDelay ?? TimeSpan.FromSeconds(10), automation.Scheduler)
				.Subscribe();
		}
	}
}