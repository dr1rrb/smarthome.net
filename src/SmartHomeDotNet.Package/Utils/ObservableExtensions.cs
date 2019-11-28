using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.SmartHome.Automations;

namespace SmartHomeDotNet.Utils
{
	/// <summary>
	/// Extensions over <see cref="IObservable{T}"/>
	/// </summary>
	public static class ObservableExtensions
	{
		/// <summary>
		/// Repeats with a delay an observable sequence until it completes successfully.
		/// </summary>
		/// <typeparam name="T">Type of the elements in the observable sequence</typeparam>
		/// <param name="source">The observable sequence to repeat</param>
		/// <param name="retryDelay">The delay to apply after an error before retry</param>
		/// <param name="scheduler">The scheduler to use to apply the delay</param>
		/// <returns>A safe observable sequence</returns>
		public static IObservable<T> Retry<T>(this IObservable<T> source, TimeSpan retryDelay, IScheduler scheduler)
			=> source.Catch(source.DelaySubscription(retryDelay, scheduler).Retry());

		/// <summary>
		/// Filters elements of an observable sequence based on predicate, and debounce changes
		/// </summary>
		/// <remarks>
		/// This mix of <see cref="Observable.Where{TSource}(System.IObservable{TSource},System.Func{TSource,bool})"/>
		/// and <see cref="Observable.DistinctUntilChanged{TSource}(System.IObservable{TSource})"/>.
		/// </remarks>
		/// <typeparam name="T">Type of elements in the observable sequence</typeparam>
		/// <param name="source">The observable sequence to filter</param>
		/// <param name="predicate">The predicate to apply to filter values</param>
		/// <returns>An observable sequence which will produce a value each time the predicate goes to `true` (only on the raising edge)</returns>
		public static IObservable<T> WhereUntilChanged<T>(this IObservable<T> source, Predicate<T> predicate)
			=> source
				.Select(value => (value: value, match: predicate(value)))
				.DistinctUntilChanged(x => x.match)
				.Where(x => x.match)
				.Select(x => x.value);

		/// <summary>
		/// Executes an asynchronous action for each (depending of the <paramref name="mode"/>) value produced by an observable sequence
		/// </summary>
		/// <typeparam name="T">Type of the element in the source observable sequence</typeparam>
		/// <param name="source">The source observable sequence</param>
		/// <param name="action">Action to execute</param>
		/// <param name="mode">
		/// Configures how to behave in case of a new element is produced by the <paramref name="source"/>
		/// while a previous execution of the <paramref name="action"/> is still pending.
		/// </param>
		/// <param name="scheduler">The scheduler to use to run <paramref name="action"/>.</param>
		/// <returns>An observable sequence of <see cref="Unit"/> which produce a value each time an execution of the action completes.</returns>
		public static IObservable<Unit> Execute<T>(this IObservable<T> source, Func<CancellationToken, T, Task> action, ConcurrentExecutionMode mode, IScheduler scheduler)
		{
			var originalAction = action;
			action = async (ct, t) =>
			{
				try
				{
					await originalAction(ct, t);
				}
				catch (OperationCanceledException)
				{
				}
			};

			switch (mode)
			{
				case ConcurrentExecutionMode.AbortPrevious:
					return source
						.Select(d => Observable.FromAsync(ct => action(ct, d), scheduler))
						.Switch();

				case ConcurrentExecutionMode.Ignore:
					var running = 0;
					return source
						.SelectMany(d => Observable.FromAsync(
							async ct =>
							{
								if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
								{
									try
									{
										await action(ct, d);
									}
									finally
									{
										running = 0;
									}
								}
							}, 
							scheduler));

				case ConcurrentExecutionMode.RunConcurrently:
					return source.SelectMany(d => Observable.FromAsync(ct => action(ct, d), scheduler));

				case ConcurrentExecutionMode.Queue:
					var @lock = new Utils.AsyncLock();
					return source
						.SelectMany(d => Observable.FromAsync(
							async ct =>
							{
								using (await @lock.LockAsync(ct))
								{
									await action(ct, d);
								}
							}, 
							scheduler));

				default:
					throw new ArgumentOutOfRangeException(nameof(mode));
			}
		}
	}

	public enum ConcurrentExecutionMode
	{
		/// <summary>
		/// Abort any previous pending execution
		/// </summary>
		AbortPrevious,

		/// <summary>
		/// Run all action in parallel
		/// </summary>
		RunConcurrently,

		/// <summary>
		/// Queue executions
		/// </summary>
		Queue,

		/// <summary>
		/// Ignore new requests if any is pending
		/// </summary>
		Ignore
	}
}