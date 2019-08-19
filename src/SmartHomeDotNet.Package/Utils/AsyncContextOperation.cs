using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Utils
{
	public sealed class AsyncContextOperation : INotifyCompletion
	{
		/// <summary>
		/// Gets an AsyncOperation that has already completed successfully
		/// </summary>
		public static AsyncContextOperation Completed { get; } = new AsyncContextOperation();

		public static AsyncContextOperation StartNew(
			Func<CancellationToken, Task> main, 
			Func<CancellationToken, Task> extent = null,
			Func<CancellationToken, Task> cancel = null) 
			=> new AsyncContextOperation(main, extent, cancel);

		public static AsyncContextOperation FromTask(Task task) => new AsyncContextOperation(main: _ => task);

		public static AsyncContextOperation FromAction(Action task, string description = null) => new AsyncContextOperation(main: async _ => task());

		public static AsyncContextOperation WhenAll(IEnumerable<AsyncContextOperation> operations) => WhenAll(operations.ToArray());
		public static AsyncContextOperation WhenAll(params AsyncContextOperation[] operations)
		{
			switch (operations.Length)
			{
				case 0: return Completed;
				case 1: return operations[0];
			}

			return new AsyncContextOperation(Main, Extend, Rollback);

			Task Main(CancellationToken ct) => Task.WhenAll(operations.Select(op => op.ToTask(TaskOptions.Main | TaskOptions.Exceptions)));
			Task Extend(CancellationToken ct) => Task.WhenAll(operations.Select(op => op.ToTask(TaskOptions.Extent | TaskOptions.Exceptions)));
			Task Rollback(CancellationToken ct) => Task.WhenAll(operations.Select(op => op.ToTask(TaskOptions.Cancel | TaskOptions.Exceptions)));
		}

		//internal static class Status
		//{
		//	// State
		//	public const int New = 1 << 0;
		//	public const int Running = 1 << 1;
		//	public const int Completed = 1 << 2;
		//	public const int Failed = 1 << 3;
		//	public const int Cancelled = 1 << 4;

		//	// Steps
		//	public const int Main = 1 << 15;
		//	public const int Extend = 1 << 16;
		//}

		[Flags]
		internal enum States
		{
			New = 0,

			// Steps
			RunningMain = 0b0001,
			RunningExtent = 0b0010,
			Cancelling = 0b0100,

			// Final states
			Completed = 0b0001_0000,
			Failed = 0b0010_0000,
			Cancelled = 0b0100_0000,
		}

		private readonly CancellationTokenSource _ct = new CancellationTokenSource();
		private readonly Func<CancellationToken, Task> _main;
		private readonly Func<CancellationToken, Task> _extent;
		private readonly Func<CancellationToken, Task> _cancel;

		private AsyncContext _context;
		private int _state = (int)States.New;
		private Exception _error;

		private ImmutableDictionary<TaskOptions, TaskHandler> _tasks = ImmutableDictionary<TaskOptions, TaskHandler>.Empty;

		// ctor for the Completed singleton
		private AsyncContextOperation()
		{
			_tasks = ImmutableDictionary<TaskOptions, TaskHandler>
				.Empty
				.WithComparers(AllEqualsComparer<TaskOptions>.Instance)
				.Add(default, TaskHandler.Completed);
		}

		private AsyncContextOperation(
			Func<CancellationToken, Task> main, 
			Func<CancellationToken, Task> extent = null,
			Func<CancellationToken, Task> cancel = null,
			string debugInfo = null)
		{
			_main = main;
			_extent = extent;
			_cancel = cancel;

			_context = AsyncContext.Current;
			if (_context == null)
			{
				this.Log().Warning(
					"An AsyncContextOperation was created out of the scope of an AsyncContext. "
					+ "This means that it will run independently and some exception might be muted silently."
					+ "If this is expected, make sure to properly handle the lifetime of this AsyncOperation "
					+ "and consider to invoke 'ToTask(Options.Extended | Options.Exceptions)' in order to handle exception.");

				Run();
			}
			else
			{
				_context.Register(this); // Will throw if context is invalid
				_ct.Token.Register(_context.Scheduler.Schedule(Run).Dispose);
			}
		}

		#region Core async operation
		internal bool IsRunning => _state < (int)States.Cancelling;

		private async void Run()
		{
			if (Interlocked.CompareExchange(ref _state, (int)States.RunningMain, (int)States.New) != (int)States.New)
			{
				// Operation is already running, do not restart it
				return;
			}
			// no needs to notify update ...

			if (_ct.IsCancellationRequested)
			{
				return;
			}

			try
			{
				await _main(_ct.Token);

				if (_extent != null)
				{
					_ct.Token.ThrowIfCancellationRequested();
					GoToState(States.RunningExtent);

					await _extent(_ct.Token);
				}

				_ct.Token.ThrowIfCancellationRequested();
				GoToState(States.Completed);
			}
			catch (OperationCanceledException)
			{
				if (_cancel != null)
				{
					GoToState(States.Cancelling);

					await _cancel(CancellationToken.None);
				}

				GoToState(States.Cancelled);
			}
			catch (Exception e)
			{
				_error = e;

				GoToState(States.Failed);
			}
		}

		internal void Cancel()
		{
			// We only abort the _ct, and let the 'Run' update the state accordingly
			_ct.Cancel();
		}

		private void GoToState(States to)
		{
			_state |= (int) to;

			// First, we update all the listening task
			foreach (var handler in _tasks.Values)
			{
				handler.Update();
			}

			// Then we notify our owning context of the state update, so it can completes its own listeners.
			_context?.UpdateState();
		}
		#endregion

		#region Ownership transfer
		/// <summary>
		/// Make sure to detach this async operation from the ambient asynchronous context
		/// that was captured when this operation was started, if any was defined.
		/// </summary>
		/// <remarks>This is useful when you want to make an async operation in a finally block</remarks>
		public void UnlinkFromAsyncContext()
		{
			_context?.UnRegister(this);
			_context = null;
		}

		/// <summary>
		/// Attaches a custom <see cref="CancellationToken"/> to this async operation.
		/// </summary>
		/// <param name="ct">The cancellation token to which this call should register</param>
		/// <returns>A <see cref="CancellationTokenRegistration"/> which allows you to manage the lifetime of this registration</returns>
		public CancellationTokenRegistration LinkTo(CancellationToken ct)
			=> ct.Register(_ct.Cancel);
		#endregion

		#region async / await support
		[Flags]
		public enum TaskOptions
		{
			/// <summary>
			/// The task will wait only for the main operation to complete, and won't raise any exception.
			/// </summary>
			Main = 1 << 0,

			/// <summary>
			/// Task will also wait for the "side effects" (like fading a light) before complete if an extend action has been provided
			/// </summary>
			/// <remarks>This implies <see cref="Main"/>.</remarks>
			Extent = 1 << 1,

			/// <summary>
			/// Task will also wait for the rollback if the operation is being cancelled and a rollback action has been provided
			/// </summary>
			/// <remarks>This implies <see cref="Extent"/>.</remarks>
			Cancel = 1 << 2,

			/// <summary>
			/// Request to the resulting task to forward the exceptions
			/// </summary>
			/// <remarks>This will also forward the <see cref="TaskCanceledException"/> if the operation was cancelled.</remarks>
			Exceptions = 1 << 31
		}

		/// <summary>
		/// Create a <see cref="Task"/> that will complete with this operations
		/// </summary>
		/// <param name="options">Configuration options for the resulting task</param>
		/// <returns>A task that will complete once this operation reach the expected state.</returns>
		public Task ToTask(TaskOptions options = TaskOptions.Extent)
			=> ImmutableInterlocked.GetOrAdd(ref _tasks, options, CreateTask).Result;

		private TaskHandler CreateTask(TaskOptions options)
			=> new TaskHandler(this, options);

		private struct TaskHandler
		{
			public static TaskHandler Completed = new TaskHandler
			{
				_syncResult = Task.CompletedTask,
				_ended = true,
			};

			private readonly AsyncContextOperation _operation;
			private readonly TaskCompletionSource<object> _asyncResult;
			private Task _syncResult;
			private readonly TaskOptions _options;
			private readonly int _endStep;
			private bool _ended;

			public Task Result => _syncResult ?? _asyncResult.Task;

			public TaskHandler(AsyncContextOperation operation, TaskOptions options)
			{
				_operation = operation;
				_options = options;
				_endStep = (int)States.RunningMain;
				if (options.HasFlag(TaskOptions.Cancel))
				{
					_endStep |= (int)(States.Cancelling | States.RunningExtent);
				}
				else if (options.HasFlag(TaskOptions.Extent))
				{
					_endStep |= (int)States.RunningExtent;
				}

				if (_operation._state > _endStep)
				{
					var (isError, isCancel) = GetResult(_operation._state, _endStep, _options);
					_syncResult = isError ? Task.FromException(operation._error)
						: isCancel ? Task.FromCanceled(_operation._ct.Token)
						: Task.CompletedTask;
					_asyncResult = default;
					_ended = true;
				}
				else
				{
					_syncResult = default;
					_asyncResult = new TaskCompletionSource<object>();
					_ended = false;
				}
			}

			public void Update()
			{
				if (_ended)
				{
					return;
				}

				if (_operation._state > _endStep)
				{
					_ended = true;

					var (isError, isCancel) = GetResult(_operation._state, _endStep, _options);
					if (isError)
					{
						_asyncResult.TrySetException(_operation._error);
					}
					else if (isCancel)
					{
						_asyncResult.TrySetCanceled();
					}
					else
					{
						_asyncResult.TrySetResult(default);
					}
				}
			}

			private static (bool isError, bool isCancel) GetResult(int state, int endStep, TaskOptions options)
			{
				var steps = state & 0b0000_0111;
				var finalState = state & 0b0111_0000;

				if (options.HasFlag(TaskOptions.Exceptions)
					&& finalState != (int)States.Completed
					&& steps == endStep) // we want only errors for the listen steps
				{
					if (finalState == (int)States.Cancelled)
					{
						return (false, true);
					}
					else
					{
						return (true, false);
					}
				}
				else
				{
					return (false, false);
				}
			}
		}

		/// <inheritdoc cref="INotifyCompletion"/>
		public TaskAwaiter GetAwaiter() => ToTask().GetAwaiter();

		/// <inheritdoc cref="INotifyCompletion"/>
		public bool IsCompleted => GetAwaiter().IsCompleted;

		/// <inheritdoc cref="INotifyCompletion"/>
		public void GetResult() => GetAwaiter().GetResult();

		/// <inheritdoc cref="INotifyCompletion"/>
		public void OnCompleted(Action continuation) => GetAwaiter().OnCompleted(continuation);

		public static implicit operator Task(AsyncContextOperation op)
			=> op.ToTask();
		#endregion
	}
}