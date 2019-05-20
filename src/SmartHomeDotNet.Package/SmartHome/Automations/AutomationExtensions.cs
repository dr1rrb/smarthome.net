using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// Extensions over <see cref="Automation"/> and <see cref="IAutomationHost"/>.
	/// </summary>
	public static class AutomationExtensions
	{
		//public static IDisposable Subscribe<T>(this IObservable<T> source, IAutomationHost host, TimeSpan? retryDelay = null)
		//{
		//	return source
		//		.Retry(retryDelay ?? Constants.DefaultRetryDelay, host.Scheduler)
		//		.Subscribe();
		//}

		//public static IDisposable Subscribe<T>(this IObservable<T> source, Automation automation, TimeSpan? retryDelay = null)
		//{
		//	return source
		//		.Retry(retryDelay ?? Constants.DefaultRetryDelay, automation.Scheduler)
		//		.Subscribe();
		//}

		//public static IDisposable Then<T>(this IObservable<T> source, Automation automation, Func<CancellationToken, T, Task> action, TimeSpan? retryDelay = null)
			//=> source
				//.Select(value => Observable.FromAsync(ct => action(ct, value)))
				//.Merge()
				//.Subscribe(automation, retryDelay);

		//public static IDisposable When<TDevice>(
		//	this Automation automation, 
		//	HomeDevice<TDevice> device, 
		//	Predicate<TDevice> predicate, 
		//	Scene sceneToStart)
		//	=> device.When(predicate).Do(_ => sceneToStart.Start()).Subscribe(automation);

		//public static IDisposable When<TDevice>(
		//	this Automation automation, 
		//	HomeDevice<TDevice> device, 
		//	Predicate<TDevice> predicate, 
		//	Func<TDevice, Task> execute, 
		//	ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious)
		//{
		//	return device.When(predicate).DoAsync(SafeExecute, mode, automation.Scheduler).Subscribe(automation);

		//	async Task SafeExecute(CancellationToken ct, TDevice d)
		//	{
		//		try
		//		{
		//			await AsyncContext.Execute(ct, () => execute(d), automation.Scheduler);
		//		}
		//		catch (Exception e)
		//		{
		//			automation.Log().Error("Execution failed", e);
		//		}
		//	}
		//}

	}
}