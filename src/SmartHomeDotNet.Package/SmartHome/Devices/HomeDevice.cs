using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// Reprsents a lazy holder of a remote device which will maintain device's state internally for fast access.
	/// </summary>
	/// <typeparam name="TDevice">The type of the device</typeparam>
	public sealed class HomeDevice<TDevice> : IObservable<TDevice>, IDisposable, IDevice<TDevice>
	{
		private readonly IConnectableObservable<(bool hasValue, TDevice value)> _source;
		private readonly BehaviorSubject<(bool hasValue, TDevice value)> _device = new BehaviorSubject<(bool, TDevice)>((false, default(TDevice)));
		private readonly SingleAssignmentDisposable _subscription = new SingleAssignmentDisposable();

		private int _isConnected;

		public HomeDevice(string id, IObservable<TDevice> source)
		{
			Id = id;
			_source = source
				.Select(device => (true, device))
				.Multicast(_device);
		}

		/// <inheritdoc />
		public string Id { get; }

		public Awaiter GetAwaiter()
		{
			Connect();

			return new Awaiter(this);
		}

		/// <inheritdoc />
		public IDisposable Subscribe(IObserver<TDevice> observer)
		{
			Connect();

			return _device
				.Where(device => device.hasValue)
				.Skip(1) // We ignore the initial state
				.Select(device => device.value)
				.Subscribe(observer);
		}

		private void Connect()
		{
			if (Interlocked.CompareExchange(ref _isConnected, 1, 0) == 0)
			{
				_subscription.Disposable = _source.Connect();
			}
		}

		/// <inheritdoc />
		public void Dispose()
			=> _subscription.Dispose();

		/// <summary>
		/// An async/await pattern awaiter optimized for the LazyDevice
		/// </summary>
		public struct Awaiter : INotifyCompletion
		{
			private readonly HomeDevice<TDevice> _owner;
			private TaskAwaiter<(bool, TDevice)>? _awaiter;

			public Awaiter(HomeDevice<TDevice> device)
			{
				_owner = device;
				_awaiter = null;
			}

			public bool IsCompleted => _owner._device.Value.hasValue;

			public TDevice GetResult()
			{
				var result = _owner._device.Value;

				return result.hasValue
					? result.value
					: throw new InvalidOperationException("This awaiter cannot run synchronously.");
			}

			public async void OnCompleted(Action continuation)
			{
				if (_owner._device.Value.hasValue)
				{
					continuation();
				}
				else
				{
					if (_awaiter == null)
					{
						_awaiter = _owner._device.FirstAsync(d => d.hasValue).ToTask(CancellationToken.None).GetAwaiter();
					}

					_awaiter.Value.OnCompleted(continuation);
				}
			}
		}
	}
}