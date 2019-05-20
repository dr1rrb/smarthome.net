using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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

		public static TDisposable DisposeWith<TDisposable>(this TDisposable disposable, SerialDisposable serial)
			where TDisposable : IDisposable
		{
			serial.Disposable = disposable;

			return disposable;
		}

		public static TDisposable DisposeWith<TDisposable>(this TDisposable disposable, CompositeDisposable disposables)
			where TDisposable : IDisposable
		{
			disposables.Add(disposable);

			return disposable;
		}
	}
}