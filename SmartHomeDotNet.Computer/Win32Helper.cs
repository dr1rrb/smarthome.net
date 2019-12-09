using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Computer
{
	internal class Win32Helper
	{
		public static Win32Helper Instance { get; } = new Win32Helper();

		private readonly object _currentGate = new object();
		private WPFApplication _current;
		private int _currentHandles = 0;

		private Win32Helper()
		{
		}

		public async Task<IWin32Window> GetOrCreateWindow(CancellationToken ct)
		{
			lock (_currentGate)
			{
				if (_current == null)
				{
					_current = new WPFApplication();
				}

				_currentHandles++;
			}

			try
			{
				var handle = await _current.GetWindow();

				return new WPFWindow(this, handle);
			}
			catch
			{
				Release();

				throw;
			}
		}

		private void Release()
		{
			lock (_currentGate)
			{
				if (--_currentHandles == 0)
				{
					_current = null;
				}
			}
		}

		public interface IWin32Window : IDisposable
		{
			IntPtr Handle { get; }

			IDisposable RegisterCallback(HwndSourceHook hook);
		}

		private class WPFApplication : IDisposable
		{
			private readonly Thread _dispatcher;
			private readonly TaskCompletionSource<Win32WindowInterop> _handle = new TaskCompletionSource<Win32WindowInterop>();

			private Application _app;

			public WPFApplication()
			{
				_dispatcher = new Thread(Run);
				_dispatcher.SetApartmentState(ApartmentState.STA);
				_dispatcher.Start();

				void Run()
				{
					_app = new Application();
					var window = new Window
					{
						Width = 100,
						Height = 100,
						Content = new TextBlock
						{
							Text = "Dummy application used to listen Win32 events",
							FontSize = 32,
							Foreground = new SolidColorBrush(Colors.Red)
						}
					};

					window.IsVisibleChanged += OnStarted;
					_app.Run(window);
				}

				async void OnStarted(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
				{
					var window = Application.Current.MainWindow;
					window.IsVisibleChanged -= OnStarted;

					_handle.TrySetResult(new Win32WindowInterop(window));

					try
					{
						// As soon as the app has been activated (so the main window handle is not 0), hide it ... but not while in the IsVisibleChanged callback
						await Task.Yield();
						window.Hide();
					}
					catch (Exception e)
					{
						this.Log().Error("Failed to hide the Win32Helper WPF Window", e);
					}
				}
			}

			public Task<Win32WindowInterop> GetWindow() => _handle.Task;

			/// <inheritdoc />
			public void Dispose()
			{
				try
				{
					if (_dispatcher.IsAlive)
					{
						_dispatcher.Abort();
					}
				}
				catch (Exception e)
				{
					this.Log().Error("Failed to abort WPF application dispatcher.");
				}

				_handle.TrySetCanceled();
			}
		}

		private struct Win32WindowInterop
		{
			public Win32WindowInterop(Window window)
			{
				Window = window;
				Interop = new WindowInteropHelper(window);
				Handle = Interop.Handle;
				Hwnd = HwndSource.FromHwnd(Handle);
			}

			public HwndSource Hwnd { get; }
			public Window Window { get; }
			public WindowInteropHelper Interop { get; }
			public IntPtr Handle { get; }
		}

		private class WPFWindow : IWin32Window
		{
			private readonly List<HwndSourceHook> _hooks = new List<HwndSourceHook>();

			private readonly Win32Helper _owner;
			private readonly Win32WindowInterop _window;

			private int _state = 0;
			

			internal WPFWindow(Win32Helper owner, Win32WindowInterop window)
			{
				_owner = owner;
				_window = window;
			}
			public IntPtr Handle => _window.Handle;

			/// <inheritdoc />
			public IDisposable RegisterCallback(HwndSourceHook hook)
			{
				_hooks.Add(hook);
				_window.Hwnd.AddHook(hook);

				return Disposable.Create(Remove);
				
				void Remove()
				{
					_window.Hwnd.RemoveHook(hook);
					_hooks.Remove(hook);
				}
			}

			/// <inheritdoc />
			public void Dispose()
			{
				if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) == 0)
				{
					foreach (var hook in _hooks)
					{
						_window.Hwnd.RemoveHook(hook);
					}

					_owner.Release();
				}
				GC.SuppressFinalize(this);
			}

			~WPFWindow()
			{
				Dispose();
			}
		}
	}
}