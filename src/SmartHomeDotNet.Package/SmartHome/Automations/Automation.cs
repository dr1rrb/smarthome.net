using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// The base class for a smart home automation
	/// </summary>
	public abstract partial class Automation : IDisposable
	{
		private readonly SerialDisposable _automationSubscription = new SerialDisposable();
		private readonly IDisposable _hostSubscription;

		/// <summary>
		/// Gets the identifier of this scene
		/// </summary>
		public string Id { get; }

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
			: this(null, name, host)
		{
		}

		/// <summary>
		/// Creates a new automation
		/// </summary>
		/// <param name="name">The friendly name of the automation</param>
		/// <param name="host">The host that manages this automation</param>
		protected Automation(string id, string name, IAutomationHost host)
		{
			Id = id ?? name;
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Scheduler = host.Scheduler ?? throw new InvalidOperationException("Host's scheduler is 'null'.");

			_hostSubscription = Observable
				.DeferAsync(async ct =>
				{
					await host.Initialized(ct, this);

					return host.GetAndObserveIsEnabled(this);
				})
				.DistinctUntilChanged()
				.Do(isEnabled =>
				{
					// Make sure to always abort the pending execution, even if 'Enable()' fails
					_automationSubscription.Disposable = null;
					if (isEnabled)
					{
						_automationSubscription.Disposable = new CompositeDisposable
						{
							// AsyncContext is used here mainly to propagate the IScheduler, but might flow in the subscriptions
							// made in the "Enable". So we make sure to dispose it only when the automation is disable.
							new AsyncContext(Scheduler),
							Enable()
						};
					}

					this.Log().Info($"Automation '{Name}' is now enabled: {isEnabled}");
				})
				.Retry(Constants.DefaultRetryDelay, Scheduler)
				.Subscribe();
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