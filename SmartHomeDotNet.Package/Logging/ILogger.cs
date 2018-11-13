using System;
using System.Linq;

namespace SmartHomeDotNet.Logging
{
	public interface ILogger
	{
		/// <summary>
		/// Writes a <see cref="F:System.Diagnostics.TraceLevel.Verbose" /> level trace event.
		/// </summary>
		/// <param name="message">The trace message.</param>
		void Debug(string message);

		/// <summary>
		/// Writes a <see cref="F:System.Diagnostics.TraceLevel.Info" /> level trace event.
		/// </summary>
		/// <param name="message">The trace message.</param>
		void Info(string message);

		/// <summary>
		/// Writes a <see cref="F:System.Diagnostics.TraceLevel.Warning" /> level trace event.
		/// </summary>
		/// <param name="message">The trace message.</param>
		void Warning(string message);

		/// <summary>
		/// Writes a <see cref="F:System.Diagnostics.TraceLevel.Error" /> level trace event.
		/// </summary>
		/// <param name="message">The trace message.</param>
		/// <param name="ex">The optional <see cref="T:System.Exception" /> for the error.</param>
		void Error(string message, Exception ex = null);
	}
}