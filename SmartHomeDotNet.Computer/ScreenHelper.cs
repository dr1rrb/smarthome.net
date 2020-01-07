using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer
{
	internal class ScreenHelper
	{
		private static ILogger _log;

		static ScreenHelper()
		{
			_log = new ScreenHelper().Log();
		}

		#region External Power settings
		[DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
		private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

		// https://docs.microsoft.com/en-us/windows/win32/power/power-setting-guids
		private static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6FE69556-704A-47A0-8F24-C28D936FDA47");
		private static Guid GUID_SESSION_DISPLAY_STATUS = new Guid("2B84C20E-AD23-4ddf-93DB-05FFBD7EFCA5");

		private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
		private const int WM_POWERBROADCAST = 0x0218;
		private const int PBT_POWERSETTINGCHANGE = 0x8013;

		private struct POWERBROADCAST_SETTING
		{
			public Guid PowerSetting;
			public uint DataLength;
			public byte Data;
		}
		#endregion

		#region External monitor state
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		private const int WM_SYSCOMMAND = 0x112;
		private const uint SC_MONITORPOWER = 0xF170;
		private enum MonitorState
		{
			MonitorStateOn = -1,
			MonitorStateOff = 2,
			MonitorStateStandBy = 1
		}
		#endregion

		#region External GetConsoleWindow
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();
		#endregion

		#region External mouse move
		[DllImport("user32.dll")]
		private static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

		private const int MOUSEEVENTF_MOVE = 0x0001;
		#endregion

		private static void SetMonitorState(MonitorState state)
			=> SendMessage(GetConsoleWindow(), WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)state);

		public static async Task Off(CancellationToken ct)
			=> SetMonitorState(MonitorState.MonitorStateOff);

		public static async Task On(CancellationToken ct)
		{
			_log.Debug("Turning screen on");

			try
			{
				mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
				await Task.Delay(40, ct);
				mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero);
			}
			catch (Exception e)
			{
				_log.Error("Failed to turn 'on' using mouse moves", e);
			}

			try
			{
				SetMonitorState(MonitorState.MonitorStateOn);
			}
			catch (Exception e)
			{
				_log.Error("Failed to turn 'on' using SendMessage", e);
			}
		}

		public static IObservable<ScreenState> GetAndObserveState()
			=> Observable
				.Create<ScreenState>(async (observer, ct) =>
				{
					var subscriptions = new CompositeDisposable();
					var window = (await Win32Helper.Instance.GetOrCreateWindow(ct)).DisposeWith(subscriptions);
					window.RegisterCallback(OnMessageReceived).DisposeWith(subscriptions);

					RegisterPowerSettingNotification(window.Handle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
					RegisterPowerSettingNotification(window.Handle, ref GUID_SESSION_DISPLAY_STATUS, DEVICE_NOTIFY_WINDOW_HANDLE);

					return subscriptions;

					IntPtr OnMessageReceived(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
					{
						if (msg == WM_POWERBROADCAST && (int)wParam == PBT_POWERSETTINGCHANGE)
						{
							var ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
							if (ps.PowerSetting == GUID_CONSOLE_DISPLAY_STATE || ps.PowerSetting == GUID_SESSION_DISPLAY_STATUS)
							{
								switch (ps.Data)
								{
									case 0: observer.OnNext(ScreenState.Off); break;
									case 1: observer.OnNext(ScreenState.On); break;
									case 2: observer.OnNext(ScreenState.Dimmed); break;
									default: _log.Info($"Unknown message: {ps.Data}"); break;
								}
							}
						}

						return IntPtr.Zero;
					}
				})
				.DistinctUntilChanged();

		public enum ScreenState : byte
		{
			Off = 0,
			On = 1,
			Dimmed = 2
		}
	}
}