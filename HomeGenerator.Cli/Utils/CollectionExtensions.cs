using System;
using System.Collections.Immutable;
using System.Linq;

namespace HomeGenerator.Cli.Utils;

public static class CollectionExtensions
{
	public static ImmutableArray<TItem> DeDuplicate<TItem, TValue>(this ImmutableArray<TItem> items, Func<TItem, TValue> selector, IEqualityComparer<TValue> comparer, Func<TItem, TItem> update)
	{
		var updatedItems = items.ToBuilder();
		foreach (var duplicates in items.GroupBy(selector, comparer).Where(group => group.Count() > 1))
		{
			foreach (var dev in duplicates)
			{
				updatedItems.Remove(dev);
				updatedItems.Add(update(dev));
			}
		}

		return updatedItems.ToImmutable();
	}

	public static ImmutableArray<TItem> DeDuplicate<TItem, TValue>(this ImmutableArray<TItem> items, Func<TItem, TValue> selector, IEqualityComparer<TValue> comparer, Func<TItem, int, TItem> update)
	{
		var updatedItems = items.ToBuilder();
		foreach (var duplicates in items.GroupBy(selector, comparer).Where(group => group.Count() > 1))
		{
			var i = 0;
			foreach (var dev in duplicates)
			{
				updatedItems.Remove(dev);
				updatedItems.Add(update(dev, i++));
			}
		}

		return updatedItems.ToImmutable();
	}
}