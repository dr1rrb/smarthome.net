using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mavri.Logging;

/// <summary>
/// Helpers to easily provide access to logging from any object
/// </summary>
public static class LogExtensions
{
	private static ILoggerProvider _provider = NullLoggerProvider.Instance;

	/// <summary>
	/// Sets the logger provider
	/// </summary>
	/// <param name="provider">The provider to use.</param>
	public static void SetProvider(ILoggerProvider provider) 
		=> _provider = provider;

	/// <summary>
	/// Gets a logger instance for the given owner.
	/// </summary>
	/// <typeparam name="T">Type of the owner.</typeparam>
	/// <param name="owner">The owner of the logger.</param>
	/// <returns>A logger instance for the given owner.</returns>
	public static ILogger Log<T>(this T owner)
		=> Holder<T>.Instance;

	private class Holder<T>
	{
		public static ILogger Instance { get; } = _provider.CreateLogger(typeof(T).FullName ?? "");
	}
}