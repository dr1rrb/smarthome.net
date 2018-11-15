using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeDotNet.Utils
{
	public static class CollectionExtensions
	{
		public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				list.Add(item);
			}
		}
	}
}