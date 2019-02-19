using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Utils
{
	partial class AsyncContext
	{
		/// <summary>
		/// Asynchronously waits for a given delay, using the <see cref="Current"/> async context to manage therading and cancellation.
		/// </summary>
		/// <remarks>If current context is not set, this delay cannot be aborted.</remarks>
		/// <param name="delay">The delay to wait before continuation</param>
		/// <returns>An asynchrnous operation that will complete at the end of the given delay</returns>
		public static async Task Delay(TimeSpan delay)
		{
			if (Current == null)
			{
				await Task.Delay(delay);
			}
			else
			{
				var tcs = new TaskCompletionSource<object>();
				using (Current.Token.Register(() => tcs.TrySetCanceled()))
				{
					Current.Scheduler.Schedule(tcs, delay, (s, t) =>
					{
						t.TrySetResult(null);
						return Disposable.Empty;
					});

					await tcs.Task;
				}
			}
		}

		/// <summary>
		/// Creates (and set as Current) a new <see cref="AsyncContext"/> which will self dispose itself as soon as
		/// the provided observable sequence produce a value.
		/// </summary>
		/// <typeparam name="TAbortTrigger">Type of the values produced by the abort observable sequence. Values are not used.</typeparam>
		/// <param name="abortTrigger">The observable sequence which triggers the abort of the result async context</param>
		/// <returns>A new AsyncContext which will be aborted when the given observable sequence produce a value</returns>
		public static AsyncContext Until<TAbortTrigger>(IObservable<TAbortTrigger> abortTrigger)
		{
			var ctx = new AsyncContext(CurrentToken, Current?.Scheduler);

			abortTrigger
				.FirstAsync()
				.Do(_ => ctx.Cancel())
				.Subscribe(_ => { }, e => abortTrigger.Log().Error("An error occurred while listening for auto abort", e), ctx.Token);

			return ctx;
		}

		/// <summary>
		/// Execute an asynchronous action wrapped into a new <see cref="AsyncContext"/>
		/// </summary>
		/// <param name="action">The action to execute</param>
		/// <param name="scheduler">The <see cref="IScheduler"/> that should be propagated through the async context</param>
		/// <returns>An async operation that will complete once the action and all its child actions has completed (cf. <see cref="WaitForCompletion"/>)</returns>
		public static async Task Execute(Func<Task> action, IScheduler scheduler = null)
		{
			using (var ctx = new AsyncContext(scheduler))
			{
				await action();
				await ctx.WaitForCompletion();
			}
		}

		/// <summary>
		/// Execute an asynchronous action wrapped into a new <see cref="AsyncContext"/>
		/// </summary>
		/// <param name="state">State to propagate to action</param>
		/// <param name="action">The action to execute</param>
		/// <param name="scheduler">The <see cref="IScheduler"/> that should be propagated through the async context</param>
		/// <returns>An async operation that will complete once the action and all its child actions has completed (cf. <see cref="WaitForCompletion"/>)</returns>
		public static async Task Execute<TState>(TState state, Func<TState, Task> action, IScheduler scheduler = null)
		{
			using (var ctx = new AsyncContext(scheduler))
			{
				await action(state);
				await ctx.WaitForCompletion();
			}
		}

		/// <summary>
		/// Execute an asynchronous action wrapped into a new <see cref="AsyncContext"/>
		/// </summary>
		/// <param name="ct">The cancellation token to which the action should be linked</param>
		/// <param name="action">The action to execute</param>
		/// <param name="scheduler">The <see cref="IScheduler"/> that should be propagated through the async context</param>
		/// <returns>An async operation that will complete once the action and all its child actions has completed (cf. <see cref="WaitForCompletion"/>)</returns>
		public static async Task Execute(CancellationToken ct, Func<Task> action, IScheduler scheduler = null)
		{
			using (var ctx = new AsyncContext(ct, scheduler))
			{
				await action();
				await ctx.WaitForCompletion();
			}
		}

		/// <summary>
		/// Execute an asynchronous action wrapped into a new <see cref="AsyncContext"/>
		/// </summary>
		/// <param name="ct">The cancellation token to which the action should be linked</param>
		/// <param name="state">State to propagate to action</param>
		/// <param name="action">The action to execute</param>
		/// <param name="scheduler">The <see cref="IScheduler"/> that should be propagated through the async context</param>
		/// <returns>An async operation that will complete once the action and all its child actions has completed (cf. <see cref="WaitForCompletion"/>)</returns>
		public static async Task Execute<TState>(CancellationToken ct, TState state, Func<TState, Task> action, IScheduler scheduler = null)
		{
			using (var ctx = new AsyncContext(ct, scheduler))
			{
				await action(state);
				await ctx.WaitForCompletion();
			}
		}
	}
}