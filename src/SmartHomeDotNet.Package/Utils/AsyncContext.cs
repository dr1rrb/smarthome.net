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

		private int _state = State.Active;
		private ImmutableHashSet<AsyncContextOperation> _operations = ImmutableHashSet<AsyncContextOperation>.Empty;

		private static class State
		{
			public const int Active = 0;
			public const int Finalizing = 1;
			public const int Disposed = 256;
		}

		/// <summary>
		/// Creates a new async context
		/// </summary>
		/// <remarks>This will still be linked to the <see cref="Current"/> if any.</remarks>
		/// <param name="scheduler">A scheduler associated to this context, if none provided, the current one will be propagated</param>
		public AsyncContext(IScheduler scheduler = null)
		{
			_previous = Current;
			if (_previous == null)
			{
				_ct = new CancellationTokenSource();
				Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler), "You cannot start a new context without scheduler. Providing 'null' is allowed only when the 'Current' AsyncContext is already set.");
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
		/// <param name="scheduler">A scheduler associated to this context, if none provided, the current one will be propagated</param>
		public AsyncContext(CancellationToken ct, IScheduler scheduler = null)
		{
			_previous = Current;
			if (_previous == null)
			{
				_ct = CancellationTokenSource.CreateLinkedTokenSource(ct);
				Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler), "You cannot start a new context without scheduler. Providing 'null' is allowed only when the 'Current' AsyncContext is already set.");
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

			UpdateState();

			return _operations.Count == 0
				? Task.CompletedTask
				: _completed.Task;
		}

		public void Register(Task task)
		{
			CheckDisposed();
			CheckCurrent();

			AsyncContextOperation.FromTask(task); // Will register itself on this context (as it's the Current)
		}

		internal void Register(AsyncContextOperation operation)
		{
			CheckDisposed();
			CheckCurrent();

			ImmutableHashSet<AsyncContextOperation> capture, updated;
			do
			{
				capture = _operations;
				updated = capture.Add(operation);

				if (capture.Count == updated.Count)
				{
					return; // operation is already present
				}

				if (_state != State.Active) // The closest to the effective add in order to reduce the risk to need a rollback
				{
					throw new InvalidOperationException("The AsyncContext is already completing, you cannot register more tasks.");
				}

			} while (Interlocked.CompareExchange(ref _operations, updated, capture) != capture);

			// The state was updated while we where adding the operation ... rollback and throw!
			if (_state != State.Active)
			{
				UnRegisterCore(operation);
				throw new InvalidOperationException("The AsyncContext is already completing, you cannot register more tasks.");
			}

			UpdateState();
		}

		internal void UnRegister(AsyncContextOperation operation)
		{
			UnRegisterCore(operation);
			UpdateState();
		}

		internal void UnRegisterCore(AsyncContextOperation operation)
		{
			ImmutableHashSet<AsyncContextOperation> capture, updated;
			do
			{
				capture = _operations;
				updated = capture.Remove(operation);

				if (capture.Count == updated.Count)
				{
					return;
				}
			} while (Interlocked.CompareExchange(ref _operations, updated, capture) != capture);
		}

		internal void UpdateState()
		{
			if (_state == State.Active)
			{
				return;
			}

			ImmutableHashSet<AsyncContextOperation> capture, updated;
			do
			{
				capture = _operations;
				updated = capture.Except(capture.Where(o => !o.IsRunning));

				if (capture.Count == updated.Count)
				{
					break;
				}

			} while (Interlocked.CompareExchange(ref _operations, updated, capture) != capture);

			if (updated.Count == 0)
			{
				_completed.TrySetResult(default);
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

				var pending = _operations;
				_operations = ImmutableHashSet<AsyncContextOperation>.Empty;

				foreach (var operation in pending)
				{
					operation.Cancel();
				}
				_ct.Cancel();
			}
		}
	}
}
