using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.SmartHome.Automations;

namespace SmartHomeDotNet.Testing
{
	public class TestAutomationHost : IAutomationHost
	{
		private readonly IScheduler _scheduler;

		private ImmutableDictionary<Automation, ReplaySubject<bool>> _isEnabled = ImmutableDictionary<Automation, ReplaySubject<bool>>.Empty;

		public TestAutomationHost(IScheduler scheduler = null)
		{
			_scheduler = scheduler ?? TaskPoolScheduler.Default;
		}

		#region IAutomationHost
		/// <inheritdoc />
		IScheduler IAutomationHost.Scheduler => _scheduler;

		/// <inheritdoc />
		async Task IAutomationHost.Initialized(CancellationToken ct, Automation automation)
		{
		}

		/// <inheritdoc />
		IObservable<bool> IAutomationHost.GetAndObserveIsEnabled(Automation automation)
			=> ImmutableInterlocked.GetOrAdd(ref _isEnabled, automation, _ => new ReplaySubject<bool>(_scheduler)).DistinctUntilChanged();
		#endregion

		public void SetIsEnabled(Automation automation, bool isEnabled)
			=> ImmutableInterlocked.GetOrAdd(ref _isEnabled, automation, _ => new ReplaySubject<bool>(_scheduler)).OnNext(isEnabled);
	}
}
