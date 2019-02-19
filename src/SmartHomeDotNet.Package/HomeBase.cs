using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.SmartHome.Scenes;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet
{
	/// <summary>
	/// The base class for a smart home
	/// </summary>
	/// <remarks>It recommended to not directly inherit from this, you should prefer to use the generic version <see cref="HomeBase{THome}"/> instead.</remarks>
	public abstract class HomeBase : IActivable, IDisposable
	{
		private int _isEnabled = 0;

		public HomeBase(IScheduler defaultScheduler = null)
		{
			Scheduler = defaultScheduler ?? TaskPoolScheduler.Default;
		}

		/// <inheritdoc />
		void IActivable.Enable() => MakeItSmart();

		public void MakeItSmart()
		{
			if (Interlocked.CompareExchange(ref _isEnabled, 1, 0) != 0)
			{
				return;
			}

			using (Initializing())
			{
				Subscriptions.AddRange(CreateHubs() ?? Enumerable.Empty<IDisposable>());
				CreateRooms();
				Subscriptions.AddRange(CreateDevices() ?? Enumerable.Empty<IDisposable>());
				Subscriptions.AddRange(CreateScenes() ?? Enumerable.Empty<IDisposable>());
				Subscriptions.AddRange(CreateAutomations() ?? Enumerable.Empty<IDisposable>());
			}
		}

		/// <summary>
		/// The <see cref="IScheduler"/> to use to managed time and asychronous wok with this home
		/// </summary>
		public IScheduler Scheduler { get; }

		/// <summary>
		/// A set of subscriptions made for this home.
		/// </summary>
		protected CompositeDisposable Subscriptions { get; } = new CompositeDisposable();

		protected virtual IDisposable Initializing() => Disposable.Empty;

		/// <summary>
		/// Invoked while initializing this home to create rooms (cf. Remarks)
		/// </summary>
		/// <remarks>This is expected to create hubs instances, so it's the first initialization method invoked, before <see cref="CreateRooms"/>.</remarks>
		/// <returns>Your hubs as <see cref="IDisposable"/> that are going to be inserted in <see cref="Subscriptions"/>.</returns>
		protected virtual IEnumerable<IDisposable> CreateHubs()
		{
			yield break;
		}

		/// <summary>
		/// Invoked while initializing this home to create rooms (cf. Remarks)
		/// </summary>
		/// <remarks>This is expected to create devices (using hubs), so it's being invoked after <see cref="CreateHubs"/> and before <see cref="CreateDevices"/>.</remarks>
		protected virtual void CreateRooms()
		{
		}

		/// <summary>
		/// Invoked while initializing this home to create home level devices (eg. weather, sun level, etc.) (cf. Remarks)
		/// </summary>
		/// <remarks>This is expected to create devices (using hubs), so it's being invoked after <see cref="CreateRooms"/> and before <see cref="CreateScenes"/>.</remarks>
		/// <returns>Your devices as <see cref="IDisposable"/> that are going to be inserted in <see cref="Subscriptions"/>.</returns>
		protected virtual IEnumerable<IDisposable> CreateDevices()
		{
			yield break;
		}

		/// <summary>
		/// Invoked while initializing this home to create <see cref="Scene"/> (cf. Remarks)
		/// </summary>
		/// <remarks>This is expected to create your scene that needs devices instances, so it's being invoked after <see cref="CreateDevices"/> and before <see cref="CreateAutomations"/>.</remarks>
		/// <returns>Your scenes as <see cref="IDisposable"/> that are going to be inserted in <see cref="Subscriptions"/>.</returns>
		protected virtual IEnumerable<IDisposable> CreateScenes()
		{
			yield break;
		}

		/// <summary>
		/// Invoked while initializing this home to create <see cref="Automation"/> (cf. Remarks)
		/// </summary>
		/// <remarks>
		/// This is expected to create your automation that needs devices and may trigger some scene,
		/// so it's the final step of the initialization process and it's being invoked after <see cref="CreateScenes"/>.
		/// </remarks>
		/// <returns>Your automations as <see cref="IDisposable"/> that are going to be inserted in <see cref="Subscriptions"/>.</returns>
		protected virtual IEnumerable<IDisposable> CreateAutomations()
		{
			yield break;
		}

		/// <inheritdoc />
		public void Dispose() => Subscriptions.Dispose();
	}
}
