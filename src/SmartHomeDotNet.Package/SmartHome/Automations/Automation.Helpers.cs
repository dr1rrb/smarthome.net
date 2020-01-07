using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Hass.Entities;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Automations
{
	partial class Automation
	{
		private static readonly TimeSpan _oneDay = TimeSpan.FromDays(1);

		protected IDisposable At(TimeSpan timeOfDay, Func<DateTimeOffset, Task> operation, ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious)
		{
			var now = Scheduler.Now.LocalDateTime;
			var day = now.Date;
			if (now.TimeOfDay > timeOfDay)
			{
				day += _oneDay;
			}

			return Observable
				.Timer(day + timeOfDay, _oneDay, Scheduler)
				.SubscribeWithContext(_ => operation(Scheduler.Now.ToLocalTime()), mode);
		}

		protected IDisposable At(IObservable<TimeSpan> timeOfDay, Func<DateTimeOffset, Task> operation, ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious)
		{
			return timeOfDay
				.DistinctUntilChanged()
				.Select(tod =>
				{
					var now = Scheduler.Now.LocalDateTime;
					var day = now.Date;
					if (now.TimeOfDay > tod)
					{
						day += _oneDay;
					}

					return Observable.Timer(day + tod, _oneDay, Scheduler);
				})
				.Switch()
				.SubscribeWithContext(_ => operation(Scheduler.Now.ToLocalTime()), mode);
		}

		//protected IDisposable When<TDevice>(HomeDevice<TDevice> device, Predicate<TDevice> predicate, Scene sceneToStart)
		//	=> device.When(predicate).Do(_ => sceneToStart.Start()).Subscribe(this);

		//protected IDisposable When<TDevice>(HomeDevice<TDevice> device, Predicate<TDevice> predicate, Func<TDevice, Task> execute, ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious)
		//{
		//	return device.When(predicate).DoAsync(SafeExecute, mode, Scheduler).Subscribe(this);

		//	async Task SafeExecute(CancellationToken ct, TDevice d)
		//	{
		//		try
		//		{
		//			using (new AsyncContext(ct, Scheduler))
		//			{
		//				await execute(d);
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			this.Log().Error("Execution failed", e);
		//		}
		//	}
		//} 
	}
}