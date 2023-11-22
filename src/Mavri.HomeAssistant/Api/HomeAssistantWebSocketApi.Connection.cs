using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Mavri.Logging;
using Mavri.Utils;
using Microsoft.Extensions.Logging;
using AsyncLock = Mavri.Utils.AsyncLock;

namespace Mavri.Ha.Api;

partial class HomeAssistantWebSocketApi
{
	private class Connection : IDisposable
	{
		private enum State
		{
			New,
			Authenticating,
			Connected,
			Disconnected,
		}

		private readonly SingleAssignmentDisposable _subscription = new();
		private readonly BehaviorSubject<State> _state = new(State.New);
		private readonly AsyncLock _sendGate = new();
		private readonly CommandId.Provider _commandIdProvider = new();

		private readonly HomeAssistantWebSocketApi _owner;
		private ClientWebSocket? _client;
		private ImmutableDictionary<int, PendingCommand> _pendingCommands = ImmutableDictionary<int, PendingCommand>.Empty;

		public Connection(HomeAssistantWebSocketApi owner)
		{
			_owner = owner;
		}

		public bool IsConnected => _state is { Value: State.Connected } && _client is { State: WebSocketState.Open };

		public void Enable(int activationDelayMs)
			=> _subscription.Disposable = new NewThreadScheduler().ScheduleAsync(TimeSpan.FromMilliseconds(activationDelayMs), Connect);

		private async Task Connect(IScheduler _, CancellationToken ct)
		{
			try
			{
				using (_client = new ClientWebSocket())
				{
					using (_state.Select(Update).Switch().Subscribe(__ => { }, e => Dispose()))
					{
						await _client.ConnectAsync(_owner._endpoint, ct);

						// We must be connected **before** ReceiveAsync

						var buffer = WebSocket.CreateClientBuffer(8 * 1024 * 1024, 128);
						while (!ct.IsCancellationRequested && _client.State == WebSocketState.Open)
						{
							var message = await _client.ReceiveAsync(buffer, ct);
							if (!message.EndOfMessage)
							{
								throw new InternalBufferOverflowException(); // TODO
							}

							switch (message.MessageType)
							{
								case WebSocketMessageType.Close:
									return;

								case WebSocketMessageType.Binary:
									this.Log().LogError("Received unknown binary message.");
									break;

								case WebSocketMessageType.Text:
									OnMessageReceived(buffer);
									break;
							}
						}
					}

					_state.OnNext(State.Disconnected); // To update the IsConnected flag
				}
			}
			catch (WebSocketException error) when (error.WebSocketErrorCode is WebSocketError.ConnectionClosedPrematurely)
			{
				_state.OnNext(State.Disconnected); // To update the IsConnected flag
				this.Log().LogInformation("Connection has been closed prematurely.");
			}
			catch (Exception e)
			{
				_state.OnNext(State.Disconnected); // To update the IsConnected flag
				this.Log().LogError("Connection failed", e);
			}
			finally
			{
				Dispose();
			}

			IObservable<Unit> Update(State state) => Observable.FromAsync(async ct2 =>
			{
				switch (state)
				{
					case State.Authenticating:
						await _client.SendAsync(new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(new AuthRequest(_owner._authToken), JsonWriteOpts)), WebSocketMessageType.Text, true, ct2);
						break;

					case State.Disconnected:
						Dispose();
						break;
				}
			});
		}

		#region Commands
		public async Task<JsonElement> Execute<TCommand>(TCommand command, CancellationToken ct)
			where TCommand : HomeAssistantCommand
		{
			using (var sender = await Send(command, ct))
			{
				return await sender.GetResult();
			}
		}

		public async Task<PendingCommand> Send<TCommand>(TCommand command, CancellationToken ct)
			where TCommand : HomeAssistantCommand
		{
			if (await _state.FirstOrDefaultAsync(state => state == State.Connected) != State.Connected)
			{
				throw new InvalidOperationException("Connection failed (invalid state).");
			}

			var client = _client;
			if (client is null)
			{
				throw new InvalidOperationException("Connection failed (no client).");
			}

			using (await _sendGate.LockAsync(ct))
			using (var id = _commandIdProvider.GetNext())
			{
				var sender = new PendingCommand(command, this, id, ct);
				var payload = JsonSerializer.SerializeToUtf8Bytes((object)command, JsonWriteOpts); // We must cast to object to not serialize only properties of HomeAssistantCommand

				await client.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, ct);

				return sender;
			}
		}

		internal void RegisterPending(PendingCommand sender)
		{
			if (!ImmutableInterlocked.TryAdd(ref _pendingCommands, sender.Id, sender))
			{
				throw new InvalidOperationException($"Invalid command ID '{sender.Id}'. ID is already present.");
			}
		}

		internal void UnRegisterPending(PendingCommand sender)
		{
			ImmutableInterlocked.TryRemove(ref _pendingCommands, sender.Id, out _);
		}
		#endregion

		#region Sink
		private void OnMessageReceived(ArraySegment<byte> buffer)
		{
			var reader = new Utf8JsonReader(buffer, true, new JsonReaderState());
			if (JsonDocument.TryParseValue(ref reader, out var doc)
				&& doc.RootElement.TryGetProperty("type", out var genericMessage))
			{
				var type = genericMessage.GetString();
				switch (type)
				{
					case "auth_ok":
						_state.OnNext(State.Connected);
						break;

					case "auth_required":
						_state.OnNext(State.Authenticating);
						break;

					case "auth_invalid":
						_state.OnNext(State.Disconnected);
						break;

					case "result":
						OnCommandResult(doc.RootElement);
						break;

					case "event":
						OnEventReceived(doc.RootElement);
						break;

					default:
						this.Log().LogError($"Received unknown message type '{type}' or failed to process content.");
						break;
				}
			}
			else
			{
				this.Log().LogError($"Cannot get the type of the received message. ('{doc?.RootElement}')");
			}
		}

		private void OnCommandResult(JsonElement msg)
		{
			if (msg.TryGetProperty("id", out var id)
				&& _pendingCommands.TryGetValue(id.GetInt32(), out var command)
				&& msg.TryGetProperty("success", out var isSuccess))
			{
				if (isSuccess.GetBoolean())
				{
					msg.TryGetProperty("result", out var result);

					command.Completed(result);
				}
				else
				{
					msg.TryGetProperty("error", out var error);

					command.Failed(error);
				}
			}
		}

		private void OnEventReceived(JsonElement msg)
		{
			if (msg.TryGetProperty("event", out var evt)
				&& evt.TryGetProperty("event_type", out var type)
				&& _owner._eventListeners.TryGetValue(type.GetString()!, out var listener))
			{
				listener.OnNext(evt);
			}
		}
		#endregion

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription.Dispose();
			_client?.Dispose();

			_owner.ConnectionAborted(this);
			_pendingCommands.Values.DisposeAllOrLog("Failed to abort a pending command.");
		}
	}
}