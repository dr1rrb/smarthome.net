using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// Provides the abstraction of a host that is able to manage <see cref="Automation"/>.
	/// </summary>
	public interface IAutomationHost
	{
		/// <summary>
		/// The scheduler to use to execute automations
		/// </summary>
		IScheduler Scheduler { get; }

		/// <summary>
		/// Notifies the host that an <see cref="Automation"/> has initialized and is now ready to operate
		/// </summary>
		/// <param name="ct">Cancellation to cancel the asynchronous action</param>
		/// <param name="automation">The automation that has is now initialized</param>
		/// <returns>An synchronous operation</returns>
		Task Initialized(CancellationToken ct, Automation automation);

		/// <summary>
		/// Gets an observable sequence of boolean which indicates if the automation is enabled or not
		/// </summary>
		/// <param name="automation">The target automation</param>
		/// <returns>An observable sequence of boolean which indicates if the automation is enabled or not</returns>
		IObservable<bool> GetAndObserveIsEnabled(Automation automation);
	}
}