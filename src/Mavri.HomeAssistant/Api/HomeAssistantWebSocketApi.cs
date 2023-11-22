using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text.Json;
using Mavri.Utils;

namespace Mavri.Ha.Api;

/// <summary>
/// Represent the web socket API of an <see cref="HomeAssistantHub"/>. (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_websocket.html"/>).
/// </summary>
public partial class HomeAssistantWebSocketApi : IDisposable
{
	internal static readonly JsonSerializerOptions JsonReadOpts = CreateDefaultJsonReadOptions();

	internal static readonly JsonSerializerOptions JsonWriteOpts = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = new SnakeCaseNamingPolicy()
	};

	public static JsonSerializerOptions CreateDefaultJsonReadOptions()
		=> new()
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
		};

	private static readonly TimeSpan _abortDelay = TimeSpan.FromSeconds(5);

	private readonly Uri _endpoint;
	private readonly string _authToken;
	private readonly IScheduler _socketScheduler;
	private readonly IScheduler _eventScheduler;

	private int _connectionUsers; // Number of users of the _connection (-1 means disposed)
	private Connection? _connection; // The current connection
	private readonly ReplaySubject<Connection?> _getAndObserveConnection; // An observable sequence to listen connection change for auto-something scenarios
	private readonly SerialDisposable _connectionAbortion = new SerialDisposable(); // Disposable used to delay the disconnection

	// Listeners are stored on the root object as they are remanent across connections
	private ImmutableDictionary<string, EventListener> _eventListeners = ImmutableDictionary<string, EventListener>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Creates a new web-socket API client to an home-assistant instance.
	/// </summary>
	/// <param name="host">The home-assistant host name (e.g. ha.myhome.net)</param>
	/// <param name="authToken">The long live auth token to connect to home-assistant.</param>
	/// <param name="socketScheduler">The scheduler used to interact with the web socket, or null to create a new <see cref="EventLoopScheduler"/>.</param>
	/// <param name="eventScheduler">The scheduler used to raise event, or null to use the <see cref="TaskPoolScheduler.Default"/>.</param>
	/// <exception cref="ArgumentNullException"></exception>
	public HomeAssistantWebSocketApi(string host, string authToken, IScheduler? socketScheduler = null, IScheduler? eventScheduler = null)
		: this(new Uri($"ws://{host}/api/websocket"), authToken, socketScheduler, eventScheduler)
	{
	}

	/// <summary>
	/// Creates a new web-socket API client to an home-assistant instance.
	/// </summary>
	/// <param name="endpoint">The home-assistant web-socket endpoint (e.g. ws://ha.myhome.net/api/websocket)</param>
	/// <param name="authToken">The long live auth token to connect to home-assistant.</param>
	/// <param name="socketScheduler">The scheduler used to interact with the web socket, or null to create a new <see cref="EventLoopScheduler"/>.</param>
	/// <param name="eventScheduler">The scheduler used to raise event, or null to use the <see cref="TaskPoolScheduler.Default"/>.</param>
	/// <exception cref="ArgumentNullException"></exception>
	public HomeAssistantWebSocketApi(Uri endpoint, string authToken, IScheduler? socketScheduler = null, IScheduler? eventScheduler = null)
	{
		_endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
		_authToken = authToken ?? throw new ArgumentNullException(nameof(authToken));
		_socketScheduler = socketScheduler ?? new EventLoopScheduler(info => new Thread(info) { Name = $"WS to {endpoint.Host}" });
		_eventScheduler = eventScheduler ?? TaskPoolScheduler.Default;

		_getAndObserveConnection = new ReplaySubject<Connection?>(1, _socketScheduler);
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
	public async Task<JsonElement> Send(HomeAssistantCommand command, CancellationToken ct)
	{
		using (EnsureConnected())
		{
			var connection = GetConnection();
			var resultJson = await connection.Execute(command, ct);

			return resultJson;
		}
	}

	/// <summary>
	/// Send a command to Home-Assistant
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	/// <param name="command">The command to send</param>
	/// <returns>A task that will complete once the command has been sent to HA with the response</returns>
	public Task<TResult?> Send<TResult>(HomeAssistantCommand command, CancellationToken ct)
		=> Send<TResult>(command, JsonReadOpts, ct);

	/// <summary>
	/// Send a command to Home-Assistant
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	/// <param name="command">The command to send</param>
	/// <returns>A task that will complete once the command has been sent to HA with the response</returns>
	public async Task<TResult?> Send<TResult>(HomeAssistantCommand command, JsonSerializerOptions jsonOptions, CancellationToken ct)
	{
		using (EnsureConnected())
		{
			var connection = GetConnection();
			var resultJson = await connection.Execute(command, ct);

			return resultJson.Deserialize<TResult>(jsonOptions);
		}
	}
	#endregion

	#region Connection management
	internal bool IsConnected([NotNullWhen(true)] out IDisposable? connectionHandled)
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
		if (currentConnection is null)
		{
			var newConnection = new Connection(this);
			currentConnection = Interlocked.CompareExchange(ref _connection, newConnection, null);
			if (currentConnection is null)
			{
				CheckDisposed();

				newConnection.Enable(activationDelayMs);
				_getAndObserveConnection.OnNext(newConnection);
				return newConnection;
			}
		}

		return currentConnection!;
	}

	private void AbortConnection()
	{
		_connectionAbortion.Disposable = _socketScheduler.Schedule(_abortDelay, () =>
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
		_connectionUsers = -1;
		_connectionAbortion.Dispose();
		_connection?.Dispose();

		(_socketScheduler as IDisposable)?.Dispose();
		(_eventScheduler as IDisposable)?.Dispose();
	}
}