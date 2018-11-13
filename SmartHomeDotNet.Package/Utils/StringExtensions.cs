using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeDotNet.Utils
{
	public static class StringExtensions
	{
		public static bool IsEmpty(this string value)
			=> string.IsNullOrWhiteSpace(value);

		public static bool HasValue(this string value)
			=> !string.IsNullOrWhiteSpace(value);

		public static string JoinBy<T>(this IEnumerable<T> values, string separator)
			=> string.Join(separator, values);
	}
}