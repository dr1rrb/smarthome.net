using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.Testing;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Tests.SmartHome.Automations
{
	[TestClass]
	public class Given_Automation
	{
		private TestScheduler _scheduler;

		[TestInitialize] public void Init() => _scheduler = new TestScheduler();
		[TestCleanup] public void Clean() => _scheduler.AdvanceTo(DateTime.MaxValue.Ticks);

		[TestMethod]
		public void When_Enable_Then_AsyncContextIsSet()
		{
			var ctx = default(AsyncContext);
			var ctxScheduler = default(IScheduler);
			var host = new TestAutomationHost(_scheduler);
			var automation = new TestAutomation(host, OnEnabled);

			_scheduler.AdvanceBy(100);
			host.SetIsEnabled(automation, true);
			_scheduler.AdvanceBy(100);

			Assert.IsNotNull(ctx);
			Assert.IsNotNull(ctxScheduler);
			Assert.AreEqual(_scheduler, ctxScheduler);

			IDisposable OnEnabled()
			{
				ctx = AsyncContext.Current;
				ctxScheduler = AsyncContext.Current?.Scheduler;

				return Disposable.Empty;
			}
		}

		[TestMethod]
		public void When_DisabledByHost_Then_SubscriptionIsAborted()
		{
			int totalActivations = 0, pendingActivations = 0;
			var host = new TestAutomationHost(_scheduler);
			var automation = new TestAutomation(host, OnEnabled);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(0, totalActivations);
			Assert.AreEqual(0, pendingActivations);

			host.SetIsEnabled(automation, true);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(1, totalActivations);
			Assert.AreEqual(1, pendingActivations);

			host.SetIsEnabled(automation, false);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(1, totalActivations);
			Assert.AreEqual(0, pendingActivations);

			IDisposable OnEnabled()
			{
				Interlocked.Increment(ref totalActivations);
				Interlocked.Increment(ref pendingActivations);

				return Disposable.Create(() => Interlocked.Decrement(ref pendingActivations));
			}
		}

		[TestMethod]
		public void When_EnabledTwiceByHost_Then_SubscriptionCreatedOnlyOnce()
		{
			int totalActivations = 0, pendingActivations = 0;
			var host = new TestAutomationHost(_scheduler);
			var automation = new TestAutomation(host, OnEnabled);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(0, totalActivations);
			Assert.AreEqual(0, pendingActivations);

			host.SetIsEnabled(automation, true);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(1, totalActivations);
			Assert.AreEqual(1, pendingActivations);

			host.SetIsEnabled(automation, true);

			_scheduler.AdvanceBy(100);
			Assert.AreEqual(1, totalActivations);
			Assert.AreEqual(1, pendingActivations);

			IDisposable OnEnabled()
			{
				Interlocked.Increment(ref totalActivations);
				Interlocked.Increment(ref pendingActivations);

				return Disposable.Create(() => Interlocked.Decrement(ref pendingActivations));
			}
		}

		[TestMethod]
		public void When_SchedulePeriodic()
		{
			_scheduler.AdvanceTo(new DateTimeOffset(1983, 9, 9, 15, 15, 0, TimeSpan.FromHours(-4)).Ticks);

			var ran = 0;
			var host = new TestAutomationHost(_scheduler);
			var automation = new TestAutomation(host);

			IDisposable subscription;
			using (new AsyncContext(_scheduler))
			{
				subscription = automation.At(new TimeSpan(22, 0, 0), Action);
			}

			_scheduler.AdvanceBy(TimeSpan.FromDays(3).Ticks);

			Assert.AreEqual(3, ran);

			subscription.Dispose(); // for perf with Cleanup() => AdvanceTo(max)

			async Task Action(DateTimeOffset arg)
			{
				Interlocked.Increment(ref ran);
			}
		}

		[TestMethod]
		public void When_SchedulePeriodic_Then_CancelSubscription()
		{
			_scheduler.AdvanceTo(new DateTimeOffset(1983, 9, 9, 15, 15, 0, TimeSpan.FromHours(-4)).Ticks);

			var ran = 0;
			var host = new TestAutomationHost(_scheduler);
			var automation = new TestAutomation(host);

			IDisposable subscription;
			using (new AsyncContext(_scheduler))
			{
				subscription = automation.At(new TimeSpan(22, 0, 0), Action);
			}

			_scheduler.AdvanceBy(TimeSpan.FromDays(3).Ticks);
			subscription.Dispose();
			_scheduler.AdvanceBy(TimeSpan.FromDays(5).Ticks);

			Assert.AreEqual(3, ran);

			async Task Action(DateTimeOffset arg)
			{
				Interlocked.Increment(ref ran);
			}
		}

		private class TestAutomation : Automation
		{
			private readonly Func<IDisposable> _onEnabled;
			
			public TestAutomation(IAutomationHost host, Func<IDisposable> onEnabled = null)
				: base(nameof(TestAutomation), host)
			{
				_onEnabled = onEnabled ?? (() => Disposable.Empty);
			}

			protected override IDisposable Enable()
				=> _onEnabled();

			public IDisposable At(TimeSpan timeOfDay, Func<DateTimeOffset, Task> operation)
				=> base.At(timeOfDay, operation);
		}
	}
}
