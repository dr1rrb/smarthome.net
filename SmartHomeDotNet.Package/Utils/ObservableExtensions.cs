using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace SmartHomeDotNet.Utils
{
	public static class ObservableExtensions
	{
		public static IObservable<T> Retry<T>(this IObservable<T> source, TimeSpan retryDelay, IScheduler scheduler)
			=> source.Catch(source.DelaySubscription(retryDelay, scheduler).Retry());

		public static IObservable<T> When<T>(this IObservable<T> source, Func<T, bool> predicate)
			=> source
				.Select(value => (value: value, match: predicate(value)))
				.DistinctUntilChanged(x => x.match)
				.Where(x => x.match)
				.Select(x => x.value);
	}
}