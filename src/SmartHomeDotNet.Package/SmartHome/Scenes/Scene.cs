using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// The base class for a smart home scene
	/// </summary>
	public abstract class Scene : IDisposable
	{
		private readonly ISceneHost _host;
		private readonly IDisposable _subscription;

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
			Id = id ?? name;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Scheduler = host.Scheduler ?? throw new InvalidOperationException("Host's scheduler is 'null'.");

			_host = host ?? throw new ArgumentNullException(nameof(host));
			_subscription = Observable
				.DeferAsync(async ct =>
				{
					await _host.Initialized(ct, this);
					await _host.SetIsRunning(ct, this, false); // Make sure to reset state at startup

					return host
						.ObserveCommands(this)
						.Select(command => command == SceneCommand.Start
							? Observable.FromAsync(SafeRun)
							: Observable.Empty<Unit>(System.Reactive.Concurrency.Scheduler.Immediate))
						.Switch();
				})
				.Subscribe(host);
		}

		private async Task SafeRun(CancellationToken ct)
		{
			try
			{
				//await _source.Publish(ct, _topic + "/state", "running");
				await _host.SetIsRunning(ct, this, true);

				await Run(ct);
			}
			catch (Exception e)
			{
				this.Log().Error("Failed to execute scene", e);
			}
			finally
			{
				// We cannot abort the completion publication
				ct = CancellationToken.None;

				//await _source.Publish(ct, _topic + "/state", "idle");
				await _host.SetIsRunning(ct, this, false);
			}
		}

		/// <summary>
		/// Performe scene actions
		/// </summary>
		/// <param name="ct">Cancellation token to use to abort a scene execution</param>
		/// <returns></returns>
		protected abstract Task Run(CancellationToken ct);

		/// <inheritdoc />
		public void Dispose() => _subscription.Dispose();
	}
}