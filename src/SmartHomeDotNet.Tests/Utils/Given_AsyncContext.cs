using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Tests.Utils
{
	[TestClass]
	public class Given_AsyncContext
	{
		private TestScheduler _scheduler;

		[TestInitialize] public void Init()
		{
			_scheduler = new TestScheduler();
			Assert.IsNull(AsyncContext.Current);
		}

		[TestCleanup] public void Clean()
		{
			_scheduler.AdvanceTo(DateTime.MaxValue.Ticks);
			Assert.IsNull(AsyncContext.Current);
		}

		[TestMethod]
		public void When_CtorDispose_Then_CurrentUpdated()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				Assert.AreEqual(ctx, AsyncContext.Current);
			}

			Assert.IsNull(AsyncContext.Current);
		}

		[TestMethod]
		public void When_None_Then_CurrentIsRemoved()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				Assert.AreEqual(ctx, AsyncContext.Current);
				using (AsyncContext.None())
				{
					Assert.IsNull(AsyncContext.Current);
				}
				Assert.AreEqual(ctx, AsyncContext.Current);
			}
		}

		[TestMethod]
		public async Task When_StartTask_Then_ContextIsFlowing()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				var taskCtx = default(AsyncContext);
				await Task.Run(() => { taskCtx = AsyncContext.Current; });

				Assert.AreEqual(ctx, taskCtx);
			}
		}

		[TestMethod]
		public async Task When_StartTaskAndCreatCtx_Then_SrcContextNotAltered()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				await Task.Run(() =>
				{
					new AsyncContext(_scheduler).ToString(); // Do not dispose this
				});

				Assert.AreEqual(ctx, AsyncContext.Current);
			}
		}

		[TestMethod]
		public async Task When_StartTaskAndRemoveCtx_Then_SrcContextNotAltered()
		{
			using (var ctx = new AsyncContext(_scheduler))
			{
				await Task.Run(() =>
				{
					AsyncContext.None(); // Do not dispose this
				});

				Assert.AreEqual(ctx, AsyncContext.Current);
			}
		}
	}
}
