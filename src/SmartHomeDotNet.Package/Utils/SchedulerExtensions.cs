using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Utils
{
	public static class SchedulerExtensions
	{
		/// <summary>
		/// Gets a new scheduler which ensures to remove the ambient <see cref="AsyncContext"/> before scheduling an asynchronous operation
		/// </summary>
		/// <param name="scheduler">The inner scheduler to use to schedule items</param>
		/// <returns>The scheduler decorator</returns>
		public static IScheduler WithoutContext(this IScheduler scheduler)
			=> new EmptyContextScheduler(scheduler);

		/// <summary>
		/// Gets a new scheduler which ensures to create a new <see cref="AsyncContext"/> before scheduling an asynchronous operation.
		/// This new context will be detached from the ambient, so it can safely be used for long running background operations.
		/// </summary>
		/// <param name="scheduler">The inner scheduler to use to schedule items</param>
		/// <returns>The scheduler decorator</returns>
		public static IScheduler WithNewContext(this IScheduler scheduler)
			=> new DetachedContextScheduler(scheduler);

		public class DetachedContextScheduler : IScheduler
		{
			private readonly IScheduler _inner;

			public DetachedContextScheduler(IScheduler inner)
			{
				_inner = inner;
			}

			/// <inheritdoc />
			public DateTimeOffset Now => _inner.Now;

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				using (new AsyncContext(_inner))
				{
					return _inner.Schedule(state, action);
				}
			}

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				using (new AsyncContext(_inner))
				{
					return _inner.Schedule(state, dueTime, action);
				}
			}

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				using (new AsyncContext(_inner))
				{
					return _inner.Schedule(state, dueTime, action);
				}
			}
		}

		public class EmptyContextScheduler : IScheduler
		{
			private readonly IScheduler _inner;

			public EmptyContextScheduler(IScheduler inner)
			{
				_inner = inner;
			}

			/// <inheritdoc />
			public DateTimeOffset Now => _inner.Now;

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				{
					return _inner.Schedule(state, action);
				}
			}

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				{
					return _inner.Schedule(state, dueTime, action);
				}
			}

			/// <inheritdoc />
			public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
			{
				using (AsyncContext.None())
				{
					return _inner.Schedule(state, dueTime, action);
				}
			}
		}
	}
}
