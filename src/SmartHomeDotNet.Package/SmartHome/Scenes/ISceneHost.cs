using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// Provides the abstraction of a host that is able to execute <see cref="Scene"/>.
	/// </summary>
	public interface ISceneHost
	{
		/// <summary>
		/// The scheduler to use to execute scenes
		/// </summary>
		IScheduler Scheduler { get; }

		/// <summary>
		/// Gets an observable sequence of the <see cref="SceneCommand"/> sent to the scene
		/// </summary>
		/// <param name="scene">The target scene of the command</param>
		/// <returns>An observable sequence of the <see cref="SceneCommand"/> sent to the scene</returns>
		IObservable<SceneCommand> ObserveCommands(Scene scene);
		
		/// <summary>
		/// Notifies the host that a <see cref="Scene"/> has initialized and is now ready to handle commands
		/// </summary>
		/// <param name="ct">Cancellation to cancel the asynchronous action</param>
		/// <param name="scene">The scene that has is now initialized</param>
		/// <returns>An synchonous operation</returns>
		Task Initialized(CancellationToken ct, Scene scene);

		/// <summary>
		/// Notifies the host that a <see cref="Scene"/> has started or completed
		/// </summary>
		/// <param name="ct"></param>
		/// <param name="scene">The scene that has started or completed</param>
		/// <param name="isRunning">A boolean which indicates if the scene is now running or not</param>
		/// <returns>An synchonous operation</returns>
		Task SetIsRunning(CancellationToken ct, Scene scene, bool isRunning);
	}
}