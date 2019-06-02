using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Utils
{
	public abstract class AsyncContextOperation : INotifyCompletion
	{
		private readonly CancellationTokenSource _ct = new CancellationTokenSource();
		private Task _task;
		private IDisposable _contextSubscription;

		// internal until we find a more robust contract than the Init !
		internal AsyncContextOperation()
		{
		}

		/// <summary>
		/// Gets the <see cref="CancellationToken"/> for this async operation
		/// </summary>
		protected CancellationToken Token => _ct.Token;

		/// <summary>
		/// This ** MUST ** be invoked in subclass constructor (yeah it's weak)
		/// </summary>
		protected void Init(Task task)
		{
			_task = task;

			var context = AsyncContext.Current;
			if (context != null)
			{
				_contextSubscription = new CompositeDisposable(
					context.Token.Register(_ct.Cancel),
					context.Register(_task));
			}
		}

		/// <summary>
		/// Make sure to detach this API call from the ambient asynchronous token
		/// that was captured when this call was started, if any was defined.
		/// </summary>
		/// <remarks>This is useful when you want to make an API in a finally block</remarks>
		public void UnlinkFromAsyncContext()
			=> _contextSubscription?.Dispose();

		/// <summary>
		/// Attaches a custom <see cref="CancellationToken"/> to this API call.
		/// </summary>
		/// <param name="ct">The cancellation token to which this call should register</param>
		/// <returns>A <see cref="CancellationTokenRegistration"/> which allows you to manage the lifetime of this registration</returns>
		public CancellationTokenRegistration LinkTo(CancellationToken ct)
			=> ct.Register(_ct.Cancel);

		/// <inheritdoc cref="INotifyCompletion"/>
		public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

		/// <inheritdoc cref="INotifyCompletion"/>
		public bool IsCompleted => GetAwaiter().IsCompleted;

		/// <inheritdoc cref="INotifyCompletion"/>
		public void GetResult() => GetAwaiter().GetResult();

		/// <inheritdoc cref="INotifyCompletion"/>
		public void OnCompleted(Action continuation) => GetAwaiter().OnCompleted(continuation);

		public static implicit operator Task(AsyncContextOperation call)
			=> call._task;
	}

	/// <summary>
	/// An asynchronous API call.
	/// </summary>
	/// <remarks>By default this will attach itself to <see cref="AsyncContext.Current"/>.</remarks>
	public sealed class ApiCall : AsyncContextOperation
	{
		/// <summary>
		/// Create a new instance
		/// </summary>
		/// <param name="client">The client to use to make the api call</param>
		/// <param name="requestUri">The uri of the request</param>
		/// <param name="payload">The payload of the request</param>
		public ApiCall(HttpClient client, string requestUri, HttpContent payload)
		{
			Init(Start(client, requestUri, payload));
		}

		private async Task Start(HttpClient client, string requestUri, HttpContent payload)
		{
			try
			{
				// Ensures that 
				if (Token.IsCancellationRequested)
				{
					return;
				}

				var response = await client.PostAsync(requestUri, payload, Token);
				response.EnsureSuccessStatusCode();
			}
			catch (Exception e)
			{
				// TODO: We should have a way to get this exception!
				this.Log().Error("Failed to send message to HA", e);
			}
		}

		///// <summary>
		///// Make sure to detach this API call from the ambient asynchronous token
		///// that was captured when this call was started, if any was defined.
		///// </summary>
		///// <remarks>This is useful when you want to make an API in a finally block</remarks>
		//public void UnlinkFromAsyncContext()
		//	=> _contextSubscription?.Dispose();

		///// <summary>
		///// Attaches a custom <see cref="CancellationToken"/> to this API call.
		///// </summary>
		///// <param name="ct">The cancellation token to which this call should register</param>
		///// <returns>A <see cref="CancellationTokenRegistration"/> which allows you to manage the lifetime of this registration</returns>
		//public CancellationTokenRegistration LinkTo(CancellationToken ct)
		//	=> ct.Register(_ct.Cancel);

		///// <inheritdoc cref="INotifyCompletion"/>
		//public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

		///// <inheritdoc cref="INotifyCompletion"/>
		//public bool IsCompleted => GetAwaiter().IsCompleted;

		///// <inheritdoc cref="INotifyCompletion"/>
		//public void GetResult() => GetAwaiter().GetResult();

		///// <inheritdoc cref="INotifyCompletion"/>
		//public void OnCompleted(Action continuation) => GetAwaiter().OnCompleted(continuation);

		//public static implicit operator Task(ApiCall call)
		//	=> call._task;
	}

	public class Transition : AsyncContextOperation
	{
		private readonly ApiCall _start;
		private readonly TimeSpan _duration;
		private readonly Func<ApiCall> _abort;

		public Transition(ApiCall start, TimeSpan duration, Func<ApiCall> abort = null)
		{
			_start = start;
			_duration = duration;
			_abort = abort;

			Init(Run());
		}

		private async Task Run()
		{
			await _start;

			try
			{
				await Task.Delay(_duration, Token);
			}
			catch (OperationCanceledException) when (_abort != null)
			{
				_abort().UnlinkFromAsyncContext();

				throw;
			}
		}

		public ApiCall GetRequest() => _start;
	}
}