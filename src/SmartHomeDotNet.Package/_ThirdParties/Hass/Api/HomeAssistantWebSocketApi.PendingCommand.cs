using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Hass.Api
{
	partial class HomeAssistantWebSocketApi
	{
		private class PendingCommand : IDisposable
		{
			private readonly Connection _connection;
			private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

			private readonly TaskCompletionSource<string> _result = new TaskCompletionSource<string>();
			private readonly CancellationTokenSource _ct;

			public PendingCommand(HomeAssistantCommand command, Connection connection, CommandId id, CancellationToken ct)
			{
				_connection = connection;
				_ct = CancellationTokenSource.CreateLinkedTokenSource(ct);
				_ct.CancelAfter(_timeout);

				Command = command;
				Id = id.Value;

				connection.RegisterPending(this);
			}

			public int Id { get; }

			public HomeAssistantCommand Command { get; }

			public Task<string> GetResult()
				=> _result.Task;

			public void Completed(JsonElement result)
			{
				_result.TrySetResult(result.GetRawText());
				_connection.UnRegisterPending(this);
			}

			public void Failed(JsonElement error)
			{
				var message = error.TryGetProperty("message", out var rawMessage)
					? rawMessage.GetString()
					: "Server sent an error without message.";

				error.TryGetProperty("code", out var code);
				var errorCode = HomeAssistantApiException.ErrorCode.Unknown;
				switch (code.GetString())
				{
					case "invalid_format": errorCode = HomeAssistantApiException.ErrorCode.Format; break;
				}

				_result.TrySetException(new HomeAssistantApiException(errorCode, message));
				_connection.UnRegisterPending(this);
			}

			/// <inheritdoc />
			public void Dispose()
			{
				try
				{
					_ct.Cancel();
					_ct.Dispose();
				}
				catch (Exception e)
				{
					this.Log().Warning($"Got an exception while cancelling command '{Command}': {e}");
				}

				_result.TrySetCanceled();
				_connection.UnRegisterPending(this);
			}
		}
	}
}