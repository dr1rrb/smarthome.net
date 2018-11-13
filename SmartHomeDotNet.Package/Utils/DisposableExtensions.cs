using System;
using System.Collections.Generic;
using System.Linq;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Utils
{
	public static class DisposableExtensions
	{
		public static void DisposeAllOrLog<T>(this IEnumerable<T> disposables, string logMessage)
			where T : IDisposable
		{
			foreach (var disposable in disposables)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception e)
				{
					disposables.Log().Error(logMessage, e);
				}
			}
		}
	}
}