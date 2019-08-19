using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeDotNet.Utils
{
	/// <summary>
	/// An equality comparer which consider all items are equals
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class AllEqualsComparer<T> : IEqualityComparer<T>
	{
		/// <summary>
		/// The singleton instance
		/// </summary>
		public static AllEqualsComparer<T> Instance { get; } = new AllEqualsComparer<T>();

		private AllEqualsComparer()
		{
		}

		public bool Equals(T x, T y) => true;
		public int GetHashCode(T obj) => 0;
	}
}