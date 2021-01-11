using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Api
{
	/// <summary>
	/// Represent the web socket API of an <see cref="HomeAssistantHub"/>. (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_websocket.html"/>).
	/// </summary>
	public partial class HomeAssistantWebSocketApi : IDisposable
	{
		internal static readonly JsonSerializerOptions JsonReadOpts = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
		};

		internal static readonly JsonSerializerOptions JsonWriteOpts = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};

		private static readonly TimeSpan _abortDelay = TimeSpan.FromSeconds(5);

		private readonly Uri _endpoint;
		private readonly string _authToken;
		private readonly IScheduler _scheduler;

		private int _connectionUsers; // Number of users of the _connection
		private Connection _connection; // The current connection
		private readonly ReplaySubject<Connection> _getAndObserveConnection; // An observable sequence to listen connection change for auto-something scenarios
		private readonly SerialDisposable _connectionAbortion = new SerialDisposable(); // Disposable used to delay the disconnection

		// Listeners are stored on teh root object as they are remanent across connections
		private ImmutableDictionary<string, EventListener> _eventListeners = ImmutableDictionary<string, EventListener>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);

		private bool _isDisposed;

		public HomeAssistantWebSocketApi(Uri endpoint, string authToken, IScheduler scheduler = null)
		{
			_endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
			_authToken = authToken ?? throw new ArgumentNullException(nameof(authToken));
			_scheduler = scheduler ?? TaskPoolScheduler.Default;

			_getAndObserveConnection = new ReplaySubject<Connection>(1, _scheduler);
		}

		#region Public API
		/// <summary>
		/// Gets an observable sequence that will produce a value each time an event
		/// of the requested type is raised by Home-Assistant.
		/// </summary>
		/// <param name="eventType">The type of events to listen</param>
		/// <returns>An observable sequence of events</returns>
		public IObservable<HomeAssistantEvent> Observe(string eventType)
			=> ImmutableInterlocked.GetOrAdd(ref _eventListeners, eventType.ToLowerInvariant(), t => new EventListener(this, t));

		/// <summary>
		/// Send a command to Home-Assistant
		/// </summary>
		/// <param name="command">The command to send</param>
		/// <returns>A task that will complete once the command has been sent to HA</returns>
		public async Task Send(HomeAssistantCommand command, CancellationToken ct)
		{
			using (EnsureConnected())
			{
				var connection = GetConnection();
				await connection.Execute(command, ct);
			}
		}

		/// <summary>
		/// Send a command to Home-Assistant
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="command">The command to send</param>
		/// <returns>A task that will complete once the command has been sent to HA with the response</returns>
		public async Task<TResult> Send<TResult>(HomeAssistantCommand command, CancellationToken ct)
		{
			using (EnsureConnected())
			{
				var connection = GetConnection();
				var resultStr = await connection.Execute(command, ct);

				return JsonSerializer.Deserialize<TResult>(resultStr, JsonReadOpts);
			}
		}
		#endregion

		#region Connection management
		internal bool IsConnected(out IDisposable connectionHandled)
		{
			if (_connectionUsers <= 0)
			{
				connectionHandled = default;
				return false;
			}
			else
			{
				connectionHandled = EnsureConnected();
				return true;
			}
		}

		private IDisposable EnsureConnected()
		{
			CheckDisposed();

			_connectionAbortion.Disposable = Disposable.Empty;
			if (Interlocked.Increment(ref _connectionUsers) == 1)
			{
				GetConnection();
			}
			return Disposable.Create(() =>
			{
				if (Interlocked.Decrement(ref _connectionUsers) == 0)
				{
					AbortConnection();
				}
			});
		}

		private Connection GetConnection(int activationDelayMs = 0)
		{
			CheckDisposed();

			var currentConnection = _connection;
			if (currentConnection == null)
			{
				var newConnection = new Connection(this);
				currentConnection = Interlocked.CompareExchange(ref _connection, newConnection, null);
				if (currentConnection == null && !_isDisposed)
				{
					newConnection.Enable(activationDelayMs);
					_getAndObserveConnection.OnNext(newConnection);
					return newConnection;
				}
			}

			return currentConnection;
		}

		private void AbortConnection()
		{
			_connectionAbortion.Disposable = _scheduler.Schedule(_abortDelay, () =>
			{
				if (_connectionUsers == 0)
				{
					// In case of currency issue, the dispose will restore re-connect
					_connection?.Dispose();
				}
			});
		}

		private void ConnectionAborted(Connection connection)
		{
			if (Interlocked.CompareExchange(ref _connection, null, connection) == connection)
			{
				_getAndObserveConnection.OnNext(null);
				if (_connectionUsers > 0)
				{
					// Oups we have to reconnect !
					GetConnection(1000); // TODO: Configured based on retry attempts
				}
			}
		}
		#endregion

		private class AuthRequest
		{
			public AuthRequest(string accessToken)
				=> AccessToken = accessToken;

			public string Type => "auth";

			public string AccessToken { get; private set; }
		}

		private void CheckDisposed()
		{
			if (_connectionUsers < 0)
			{
				throw new ObjectDisposedException(nameof(HomeAssistantWebSocketApi));
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_isDisposed = true;
			_connectionUsers = -1;
			_connectionAbortion.Dispose();
			_connection?.Dispose();
		}
	}
}
