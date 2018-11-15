using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// The base class for a smart home automation
	/// </summary>
	public abstract class Automation : IDisposable
	{
		private readonly SerialDisposable _automationSubscription = new SerialDisposable();
		private readonly IDisposable _hostSubscription;

		/// <summary>
		/// The name of the automation
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The scheduler to use to execute automation
		/// </summary>
		protected internal IScheduler Scheduler { get; }

		/// <summary>
		/// Creates a new automation
		/// </summary>
		/// <param name="name">The friendly name of the automation</param>
		/// <param name="host">The host that manages this automation</param>
		protected Automation(string name, IAutomationHost host)
		{
			Name = name;
			Scheduler = host.Scheduler;

			_hostSubscription = host
				.GetAndObserveIsEnabled(this)
				.Do(isEnabled =>
				{
					// Make sure to always abort the pending execution, even if 'Enabled()' fails
					_automationSubscription.Disposable = null;
					if (isEnabled)
					{
						_automationSubscription.Disposable = Enable();
					}
				})
				.Subscribe(host);
		}

		/// <summary>
		/// Enables the automation
		/// </summary>
		/// <returns>A disposable that will disable the automation when disposed</returns>
		protected abstract IDisposable Enable();

		/// <inheritdoc />
		public void Dispose()
		{
			_hostSubscription.Dispose();
			_automationSubscription.Dispose();
		}
	}
}