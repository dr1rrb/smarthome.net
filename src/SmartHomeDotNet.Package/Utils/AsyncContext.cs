using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeDotNet.Utils
{
	/// <summary>
	/// Represent the context of a set of asynchronous operations
	/// </summary>
	public sealed partial class AsyncContext : IDisposable
	{
		private static readonly AsyncLocal<AsyncContext> _current = new AsyncLocal<AsyncContext>();

		/// <summary>
		/// Remove the <see cref="Current"/> AsyncContext from the stack, so async operation will run in a detached mode.
		/// </summary>
		/// <remarks>
		/// This is useful to execute some async operations in a catch or a finally block after
		/// the <see cref="Current"/> has been cancelled.
		/// </remarks>
		/// <returns>A disposable to handle the scope of this override</returns>
		public static IDisposable None()
		{
			var original = Current;
			if (original == null)
			{
				return Disposable.Empty;
			}

			_current.Value = null;

			return Disposable.Create(Restore);

			void Restore()
			{
				if (Current != null)
				{
					throw new InvalidOperationException("Invalid stack");
				}

				_current.Value = original;
			}
		}

		/// <summary>
		/// Gets the current async context
		/// </summary>
		public static AsyncContext Current => _current.Value;

		/// <summary>
		/// Gets the <see cref="Token"/> of the <see cref="Current"/> async context if any defined,
		/// or <see cref="CancellationToken.None"/> is no current context defined.
		/// </summary>
		public static CancellationToken CurrentToken => _current.Value?.Token ?? CancellationToken.None;

		private readonly TaskCompletionSource<object> _completed = new TaskCompletionSource<object>();
		private readonly AsyncContext _previous;
		private readonly CancellationTokenSource _ct;

		private int _pending;
		private int _state = State.Active;
		private ImmutableList<AggregateException> _exceptions = ImmutableList<AggregateException>.Empty;

		private static class State
		{
			public const int Active = 0;
			public const int Finalizing = 1;
			public const int Disposed = 2;
		}

		/// <summary>
		/// Creates a new async context
		/// </summary>
		/// <remarks>This will still be linked to the <see cref="Current"/> if any.</remarks>
		/// <param name="scheduler">An optional scheduler associated to this context</param>
		public AsyncContext(IScheduler scheduler = null)
		{
			_previous = Current;
			if (_previous == null)
			{
				_ct = new CancellationTokenSource();
				Scheduler = scheduler;
			}
			else
			{
				_ct = CancellationTokenSource.CreateLinkedTokenSource(_previous.Token);
				Scheduler = scheduler ?? _previous.Scheduler;
			}

			_current.Value = this;
		}

		/// <summary>
		/// Creates an async context linked to a given <see cref="CancellationToken"/>.
		/// </summary>
		/// <remarks>This will still be linked to the <see cref="Current"/> if any.</remarks>
		/// <param name="ct">The cancellation token to link to</param>
		/// <param name="scheduler">An optional scheduler associated to this context</param>
		public AsyncContext(CancellationToken ct, IScheduler scheduler = null)
		{
			_previous = Current;
			if (_previous == null)
			{
				_ct = CancellationTokenSource.CreateLinkedTokenSource(ct);
				Scheduler = scheduler;
			}
			else
			{
				_ct = CancellationTokenSource.CreateLinkedTokenSource(ct, _previous.Token);
				Scheduler = scheduler ?? _previous.Scheduler;
			}

			_current.Value = this;
		}

		/// <summary>
		/// Gets the <see cref="CancellationToken"/> of this context
		/// </summary>
		/// <remarks>This should be used by each task that being registered on this context.</remarks>
		public CancellationToken Token => _ct.Token;

		/// <summary>
		/// Gets the <see cref="IScheduler"/> associated with this async context, if any
		/// </summary>
		public IScheduler Scheduler { get; }

		/// <summary>
		/// Cancel the current context and all tasks attached to it (cancels the <see cref="Token"/>)
		/// </summary>
		/// <remarks>This won't affect the <see cref="Current"/> context.</remarks>
		public void Cancel() 
			=> _ct.Cancel();

		/// <summary>
		/// Gets an awaiter that completes when all asynchronous operations registered
		/// on this context are completed.
		/// </summary>
		/// <remarks>This will freeze this context an prevent any usage of the <see cref="Register"/>.</remarks>
		/// <remarks>
		/// This method is accessible even if the context has been disposed, however all tasks should have been cancelled
		/// (if they where properly built with the <see cref="Token"/>).
		/// </remarks>
		public Task WaitForCompletion()
		{
			Interlocked.CompareExchange(ref _state, State.Finalizing, State.Active);

			return _pending == 0
				? Task.CompletedTask
				: _completed.Task;
		}

		/// <summary>
		/// Registers an asynchronous operation on this context
		/// </summary>
		/// <param name="task">The asynchronous operation</param>
		public IDisposable Register(Task task)
		{
			CheckDisposed();
			CheckCurrent();

			if (task.IsCompleted)
			{
				TouchException(task);
				return Disposable.Empty;
			}

			// We increment counter BEFORE validating state, so we avoid concurrency issue
			// with the 'WaitForCompletion' (which does the opposite)
			Interlocked.Increment(ref _pending);

			if (_state != State.Active)
			{
				ReleaseOne();
				throw new InvalidOperationException("The AsyncContext is already completing, you cannot register more tasks.");
			}

			var release = Disposable.Create(ReleaseOne);

			task.ContinueWith(Release, (this, release), TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously);

			return release;
		}

		private void ReleaseOne()
		{
			// Here again we make sure to change '_pending' then '_state' to avoid concurrency issue
			// with the 'WaitForCompletion' (which does the opposite)
			if (Interlocked.Decrement(ref _pending) == 0 && _state != State.Active)
			{
				_completed.TrySetResult(null);
			}
		}

		private static void Release(Task t, object state)
		{
			var (that, registration) = (((AsyncContext, IDisposable))state);

			that.TouchException(t);
			registration.Dispose();
		}

		private void TouchException(Task task)
		{
			if (task.IsFaulted)
			{
				do
				{
					var captured = _exceptions;
					var updated = captured.Add(task.Exception);
					if (Interlocked.CompareExchange(ref _exceptions, updated, captured) == captured)
					{
						return;
					}
				} while (true);
			}
		}

		private void CheckCurrent([CallerMemberName] string method = null)
		{
			if (Current != this)
			{
				throw new InvalidOperationException($"You can use '{method}' on an async context only while it is the current one.");
			}
		}

		private void CheckDisposed()
		{
			if (_state == State.Disposed)
			{
				throw new ObjectDisposedException(nameof(AsyncContext));
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			CheckCurrent();

			if (Interlocked.Exchange(ref _state, State.Disposed) != State.Disposed)
			{
				_current.Value = _previous;
				_ct.Cancel();
			}
		}
	}
}
