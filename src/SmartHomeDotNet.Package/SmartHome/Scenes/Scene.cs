using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// The base class for a smart home scene
	/// </summary>
	public abstract class Scene : IDisposable
	{
		private readonly SerialDisposable _pending = new SerialDisposable();

		private readonly ISceneHost _host;
		private readonly IDisposable _subscription;

		private int _isRunning;

		public enum RunOption
		{
			/// <summary>
			/// When <see cref="Scene.Run"/> is invoked on a scene, if <see cref="Scene.IsRunning"/> then the execution will be ignored.
			/// </summary>
			IgnoreIfRunning = 0,

			/// <summary>
			/// When <see cref="Scene.Run"/> is invoked on a scene, if <see cref="Scene.IsRunning"/> then the pending
			/// execution will be aborted, and a new instance will be started.
			/// </summary>
			AbortPending,

			// AttachToPending
			// AttachToPendingWithCancellationToken
		}

		/// <summary>
		/// Gets the identifier of this scene
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Gets the name of this scene
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The scheduler to use to execute the scene
		/// </summary>
		protected internal IScheduler Scheduler { get; }

		/// <summary>
		/// Creates a new scene
		/// </summary>
		/// <param name="name">The friendly name of the scene</param>
		/// <param name="host">The host that manages this scene</param>
		protected Scene(string name, ISceneHost host)
			: this(null, name, host)
		{
		}

		/// <summary>
		/// Creates a new scene
		/// </summary>
		/// <param name="name">The friendly name of the scene</param>
		/// <param name="host">The host that manages this scene</param>
		protected Scene(string id, string name, ISceneHost host)
		{
			host = host ?? throw new ArgumentNullException(nameof(host));

			Id = id ?? name;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Scheduler = host.Scheduler ?? throw new InvalidOperationException("Host's scheduler is 'null'.");

			_host = host;
			_subscription = Scheduler.ScheduleAsync(async (scheduler, ct) =>
			{
				await _host.Initialized(ct, this);
				await _host.SetIsRunning(ct, this, false); // Make sure to reset state at startup

				return host
					.ObserveCommands(this)
					.Do(HandleCommand)
					.Retry(Constants.DefaultRetryDelay, Scheduler)
					.Subscribe();

				void HandleCommand(SceneCommand command)
				{
					this.Log().Info($"[{Id}] Received '{command}'");

					if (command == SceneCommand.Start)
					{
						Start();
					}
					else
					{
						Stop();
					}
				}
			});
		}

		/// <summary>
		/// Gets a boolean which indicates if this scene is currently running or not
		/// </summary>
		public bool IsRunning => _isRunning > 0;

		/// <summary>
		/// Executes the scene now
		/// </summary>
		/// <remarks>This allows you to track the completion of the scene. If you don't want to handle the asynchronous execution, you should use <see cref="Start"/>.</remarks>
		/// <param name="ct">Cancellation token to abort the execution</param>
		/// <param name="options">Configures the behavior if this scene <see cref="IsRunning"/>.</param>
		/// <returns>An asynchronous operation that will complete when this scene is over.</returns>
		public async Task Run(CancellationToken ct, RunOption options = RunOption.IgnoreIfRunning)
		{
			try
			{
				if (IsRunning && options == RunOption.IgnoreIfRunning)
				{
					return;
				}

				// Set _isRunning prior to abort pending, so there is no flicker of 'IsRunning'
				// Note: we use a int as at this point we already have a running instance which will be aborted below
				Interlocked.Increment(ref _isRunning);

				using (var ctx = new AsyncContext(ct, Scheduler))
				{
					// This will abort any other pending
					_pending.Disposable = Disposable.Create(ctx.Cancel);

					await _host.SetIsRunning(ctx.Token, this, true);

					await Run();
					await ctx.WaitForCompletion();
				}
			}
			finally
			{
				// We cannot abort the completion publication
				ct = CancellationToken.None;

				if (Interlocked.Decrement(ref _isRunning) == 0)
				{
					await _host.SetIsRunning(ct, this, false);
				}
			}
		}

		/// <summary>
		/// Request to start the scene
		/// </summary>
		/// <remarks>This is a "fire and forget". If you need to track the execution, you should use <see cref="Run"/>.</remarks>
		public void Start()
		{
			// This method is fire and forget, prevent flowing of the AsyncContext
			using (AsyncContext.None())
			{
				Scheduler.ScheduleAsync(async (scheduler, ct) =>
				{
					try
					{
						await Run(ct);
					}
					catch (Exception e)
					{
						this.Log().Error($"Failed to execute scene '{Name}'", e);
					}
				});
			}
		}

		/// <summary>
		/// Aborts any pending execution (cf. <see cref="IsRunning"/>)
		/// </summary>
		/// <remarks>This will abort the execution not matter it was started using <see cref="Run"/> or <see cref="Start"/>.</remarks>
		public void Stop()
			=> _pending.Disposable = Disposable.Empty;

		/// <summary>
		/// Perform scene actions
		/// </summary>
		/// <remarks>
		/// This method uses the <see cref="AsyncContext"/> to track execution and handle abort.
		/// If needed, you can get access to a <see cref="CancellationToken"/> from <see cref="AsyncContext.CurrentToken"/>.
		/// </remarks>
		/// <returns>An asynchronous operation that will complete at the end of the scene actions</returns>
		protected abstract Task Run();

		/// <inheritdoc />
		public void Dispose() => _subscription.Dispose();
	}
}