using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Mavri.Logging;
using Microsoft.Extensions.Logging;

namespace Mavri.Utils;

/// <summary>
/// Helpers to manipulate <see cref="IDisposable"/> objects.
/// </summary>
public static class DisposableExtensions
{
	/// <summary>
	/// Dispose the given disposable or log exception if the dispose method throws an exception.
	/// </summary>
	/// <param name="disposable">The disposable to dispose.</param>
	/// <param name="logMessage">The message to log in case of error in dispose.</param>
	public static void DisposeOrLog(this IDisposable disposable, string logMessage)
	{
		try
		{
			disposable.Dispose();
		}
		catch (Exception e)
		{
			disposable.Log().LogError(logMessage, e);
		}
	}

	/// <summary>
	/// Dispose a set of disposables objects or log exception for each exception thrown by the dispose method.
	/// </summary>
	/// <param name="disposables">The set of disposable to dispose.</param>
	/// <param name="logMessage">The message to log in case of error in dispose.</param>
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
				disposable.Log().LogError(logMessage, e);
			}
		}
	}

	/// <summary>
	/// Registers a disposable to be disposed by a <see cref="SerialDisposable"/>.
	/// </summary>
	/// <typeparam name="TDisposable">Type of the disposable to register.</typeparam>
	/// <param name="disposable">The disposable to register.</param>
	/// <param name="serial">The serial disposable onto which the given disposable should be registered.</param>
	/// <returns>The provided disposable for fluent usage.</returns>
	public static TDisposable DisposeWith<TDisposable>(this TDisposable disposable, SerialDisposable serial)
		where TDisposable : IDisposable
	{
		serial.Disposable = disposable;

		return disposable;
	}

	/// <summary>
	/// Registers a disposable to be disposed with a <see cref="CompositeDisposable"/>.
	/// </summary>
	/// <typeparam name="TDisposable">Type of the disposable to register.</typeparam>
	/// <param name="disposable">The disposable to register.</param>
	/// <param name="disposables">The composite disposable into which the given disposable should be registered.</param>
	/// <returns>The provided disposable for fluent usage.</returns>
	public static TDisposable DisposeWith<TDisposable>(this TDisposable disposable, CompositeDisposable disposables)
		where TDisposable : IDisposable
	{
		disposables.Add(disposable);

		return disposable;
	}
}