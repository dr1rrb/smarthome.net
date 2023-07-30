using System;
using System.Linq;

namespace SmartHomeDotNet.Logging;

internal class SerilogAdapter : ILogger
{
	public static SerilogAdapter Instance { get; } = new SerilogAdapter();

	private SerilogAdapter() { }

	/// <inheritdoc />
	public void Debug(string message) => Serilog.Log.Logger.Debug(message);

	/// <inheritdoc />
	public void Info(string message) => Serilog.Log.Logger.Information(message);

	/// <inheritdoc />
	public void Warning(string message) => Serilog.Log.Logger.Warning(message);

	/// <inheritdoc />
	public void Error(string message, Exception? ex = null) => Serilog.Log.Logger.Error(ex, message);
}