using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Tests.Utils
{
	[TestClass]
	public class Given_AsyncContextOperation
	{
		private TestScheduler _scheduler;

		[TestInitialize]
		public void Init()
		{
			_scheduler = new TestScheduler();
			Assert.IsNull(AsyncContext.Current);
		}

		[TestCleanup]
		public void Clean()
		{
			_scheduler.AdvanceTo(DateTime.MaxValue.Ticks);
			Assert.IsNull(AsyncContext.Current);
		}

		[TestMethod]
		public void When_CreateOp_Then_IsAttachedToAmbientAsyncContext()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				var op = AsyncContextOperation.FromTask(Task.CompletedTask);

				var opCtx = op.GetType().GetField("_context", BindingFlags.Instance| BindingFlags.NonPublic).GetValue(op);
				var ctxOps = ctx.GetType().GetField("_operations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctx) as IEnumerable<AsyncContextOperation>;

				Assert.AreEqual(ctx, opCtx);
				Assert.IsTrue(ctxOps?.Contains(op) ?? false);
			}
		}

		[TestMethod]
		public void When_CreateOp_Then_CurrentContextWaitsForIt()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				var result = new TaskCompletionSource<object>();
				var op = AsyncContextOperation.StartNew(ct => result.Task);
				_scheduler.AdvanceBy(100);
				var end = ctx.WaitForCompletion();

				Assert.AreEqual(TaskStatus.WaitingForActivation, end.Status);
				result.SetResult(default);

				Assert.AreEqual(TaskStatus.RanToCompletion, end.Status);
			}
		}

		[TestMethod]
		public async Task When_CreateOp_Then_CurrentContextWaitsForItAndDontGetSyncException()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				var op = AsyncContextOperation.StartNew(ct => throw new DivideByZeroException());
				_scheduler.AdvanceBy(100);
				var end = ctx.WaitForCompletion();

				Assert.AreEqual(TaskStatus.RanToCompletion, end.Status);
			}
		}

		[TestMethod]
		public void When_CreateOp_Then_CurrentContextWaitsForItAndDontGetASyncException()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				var op = AsyncContextOperation.StartNew(async ct =>
				{
					await _scheduler.Yield(ct);
					throw new DivideByZeroException();
				});
				_scheduler.AdvanceBy(100);
				var end = ctx.WaitForCompletion();

				Assert.AreEqual(TaskStatus.RanToCompletion, end.Status);
			}
		}

		[TestMethod]
		public async Task When_ToTask_Then_WaitOnlyForRequestedSteps()
		{
			var main = new TaskCompletionSource<object>();
			var extent = new TaskCompletionSource<object>();
			var cancel = new TaskCompletionSource<object>();

			var op = AsyncContextOperation.StartNew(_ => main.Task, _ => extent.Task, _ => cancel.Task);
			var mainTask = op.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = op.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = op.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			extent.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}

		[TestMethod]
		public async Task When_ToTask_Then_WaitOnlyForRequestedStepsWhenExceptionInMain()
		{
			var main = new TaskCompletionSource<object>();
			var extent = new TaskCompletionSource<object>();
			var cancel = new TaskCompletionSource<object>();

			var op = AsyncContextOperation.StartNew(_ => main.Task, _ => extent.Task, _ => cancel.Task);
			var mainTask = op.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = op.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = op.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main.SetException(new DivideByZeroException());

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}

		[TestMethod]
		public async Task When_ToTask_Then_WaitOnlyForRequestedStepsWhenExceptionInExtent()
		{
			var main = new TaskCompletionSource<object>();
			var extent = new TaskCompletionSource<object>();
			var cancel = new TaskCompletionSource<object>();

			var op = AsyncContextOperation.StartNew(_ => main.Task, _ => extent.Task, _ => cancel.Task);
			var mainTask = op.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = op.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = op.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			extent.SetException(new DivideByZeroException());

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}

		[TestMethod]
		public async Task When_ToTask_Then_WaitOnlyForRequestedStepsWhenMainCancelled()
		{
			var main = new TaskCompletionSource<object>();
			var extent = new TaskCompletionSource<object>();
			var cancel = new TaskCompletionSource<object>();

			var op = AsyncContextOperation.StartNew(_ => main.Task, _ => extent.Task, _ => cancel.Task);
			var mainTask = op.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = op.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = op.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main.SetCanceled();

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			cancel.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}

		[TestMethod]
		public async Task When_ToTask_Then_WaitOnlyForRequestedStepsWhenExtentCancelled()
		{
			var main = new TaskCompletionSource<object>();
			var extent = new TaskCompletionSource<object>();
			var cancel = new TaskCompletionSource<object>();

			var op = AsyncContextOperation.StartNew(_ => main.Task, _ => extent.Task, _ => cancel.Task);
			var mainTask = op.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = op.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = op.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			extent.SetCanceled();

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			cancel.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}

		[TestMethod]
		public void When_Combine_Then_ToTaskWaitsForAll()
		{
			var main1 = new TaskCompletionSource<object>();
			var extent1 = new TaskCompletionSource<object>();
			var cancel1 = new TaskCompletionSource<object>();
			var main2 = new TaskCompletionSource<object>();
			var extent2 = new TaskCompletionSource<object>();
			var cancel2 = new TaskCompletionSource<object>();
			var op1 = AsyncContextOperation.StartNew(_ => main1.Task, _ => extent1.Task, _ => cancel1.Task);
			var op2 = AsyncContextOperation.StartNew(_ => main2.Task, _ => extent2.Task, _ => cancel2.Task);
			var sut = AsyncContextOperation.WhenAll(op1, op2);

			var mainTask = sut.ToTask(AsyncContextOperation.TaskOptions.Main);
			var extentTask = sut.ToTask(AsyncContextOperation.TaskOptions.Extent);
			var cancelTask = sut.ToTask(AsyncContextOperation.TaskOptions.Cancel);

			main1.SetResult(default);
			extent1.SetResult(default);

			Assert.AreEqual(TaskStatus.WaitingForActivation, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			main2.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, extentTask.Status);
			Assert.AreEqual(TaskStatus.WaitingForActivation, cancelTask.Status);

			extent2.SetResult(default);

			Assert.AreEqual(TaskStatus.RanToCompletion, mainTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, extentTask.Status);
			Assert.AreEqual(TaskStatus.RanToCompletion, cancelTask.Status);
		}
	}
}