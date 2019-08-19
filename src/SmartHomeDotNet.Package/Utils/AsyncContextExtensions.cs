using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.SmartHome;

namespace SmartHomeDotNet.Utils
{
	public static class AsyncContextExtensions
	{
		public static IDisposable SubscribeWithContext<T>(this IObservable<T> source, TimeSpan? retryDelay = null)
		{
			var ctx = AsyncContext.Current;
			if (ctx == null)
			{
				throw new InvalidOperationException("This method can only be used in the scope of an AsyncContext.");
			}

			return source.Retry(retryDelay ?? Constants.DefaultRetryDelay, ctx.Scheduler).Subscribe();
		}

		public static IDisposable SubscribeWithContext<T>(
			this IObservable<T> source, 
			Func<T, Task> action, 
			ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious,
			TimeSpan? retryDelay = null)
		{
			var ctx = AsyncContext.Current;
			if (ctx == null)
			{
				throw new InvalidOperationException("This method can only be used in the scope of an AsyncContext.");
			}

			return source
				.Execute(Execute, mode, ctx.Scheduler)
				.Retry(retryDelay ?? Constants.DefaultRetryDelay, ctx.Scheduler)
				.Subscribe();

			Task Execute(CancellationToken ct, T value)
				=> AsyncContext.Execute(ct, value, action, ctx.Scheduler);
		}
	}
}
