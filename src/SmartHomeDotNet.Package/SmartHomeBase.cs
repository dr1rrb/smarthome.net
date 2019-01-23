using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet
{
	public class SmartHomeBase : IActivable, IDisposable
	{
		private int _isEnabled = 0;

		public SmartHomeBase(IScheduler defaultScheduler = null)
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
				Subscriptions.AddRange(CreateScenes() ?? Enumerable.Empty<IDisposable>());
				Subscriptions.AddRange(CreateAutomations() ?? Enumerable.Empty<IDisposable>());
			}
		}

		public IScheduler Scheduler { get; }

		protected CompositeDisposable Subscriptions { get; } = new CompositeDisposable();

		protected virtual IDisposable Initializing() => Disposable.Empty;

		protected virtual IEnumerable<IDisposable> CreateHubs()
		{
			yield break;
		}

		protected virtual void CreateRooms()
		{
		}

		protected virtual IEnumerable<IDisposable> CreateScenes()
		{
			yield break;
		}

		protected virtual IEnumerable<IDisposable> CreateAutomations()
		{
			yield break;
		}

		/// <inheritdoc />
		public void Dispose() => Subscriptions.Dispose();
	}
}
