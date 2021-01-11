#nullable enable

using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NAudio.CoreAudioApi;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Devices_2;
using DeviceState = NAudio.CoreAudioApi.DeviceState;
using IDeviceHost = SmartHomeDotNet.SmartHome.Devices.IDeviceHost;

namespace SmartHomeDotNet.Computer.Audio
{
	//[AttributeUsage(AttributeTargets.Class)]
	//public class DeviceProviderAttribute : Attribute
	//{
	//	public DeviceProviderAttribute(Type deviceIdentifierType)
	//	{
	//	}
	//}

	public class Microphone : Device, ISupport<TurnOn>, ISupport<TurnOff>, IDeviceState
	{
		public Microphone(string id, string name, float volume, bool muted)
		{
			Id = id;
			Name = name;
			Volume = volume;
			Muted = muted;
		}

		public new string Id { get; }

		public string Name { get; }

		public float Volume { get; }

		public bool Muted { get; }

		public bool IsPersistent { get; } = true;

		public DeviceStatus DeviceStatus { get; set; }
	}

	//public class AudioDeviceHost : IDeviceHost<AudioDeviceIdentifier, Microphone>, IDeviceHost<AudioDeviceIdentifier, Speaker>
	//{
	//	/// <inheritdoc />
	//	IObservable<Microphone> IDeviceHost<AudioDeviceIdentifier, Microphone>.Bind(AudioDeviceIdentifier identifier)
	//		=> Observable.Return(new Microphone());

	//	/// <inheritdoc />
	//	AsyncContextOperation IDeviceHost<AudioDeviceIdentifier, Speaker>.Execute(ICommand command, params AudioDeviceIdentifier[] devices)
	//		=> throw new NotImplementedException();

	//	/// <inheritdoc />
	//	IObservable<Speaker> IDeviceHost<AudioDeviceIdentifier, Speaker>.Bind(AudioDeviceIdentifier identifier)
	//		=> Observable.Return(new Speaker());

	//	/// <inheritdoc />
	//	AsyncContextOperation IDeviceHost<AudioDeviceIdentifier, Microphone>.Execute(ICommand command, params AudioDeviceIdentifier[] devices)
	//		=> throw new NotImplementedException();
	//}

	//public class AudioDevice
	//{
	//	private readonly AudioDeviceIdentifier _id;

	//	public AudioDevice(AudioDeviceIdentifier id)
	//	{
	//		_id = id;
	//	}

	//	///// <inheritdoc />
	//	//public IObservable<SmartHome.Devices.DeviceState> GetAndObserveState()
	//	//	=> Observable.Using(() => new DeviceListener(_name), listener => listener.Select(dev => dev.State)
	//	//	{

	//	//	})

	//	///// <inheritdoc />
	//	//public AsyncContextOperation Execute(ICommand command)
	//	//	=> throw new NotImplementedException();

	//	//public IObservable<SmartHome.Devices.DeviceState> ObserveState(MMDevice device)
	//	//{
	//	//	if (device is null)
	//	//	{

	//	//	}
	//	//}
		
	//	/// <inheritdoc />
	//	//public IObservable<TState> Bind<TState>(AudioDeviceIdentifier identifier)
	//	//{

	//	//}

	//	///// <inheritdoc />
	//	//public AsyncContextOperation Execute(ICommand command, params AudioDeviceIdentifier[] devices)
	//	//	=> throw new NotImplementedException();
	//}

	////public class DeviceHostHelper
	////{
	////	IObservable<TState> Cast<TSta>()
	////}

	//public class AudioHelper
	//{
	//	//[DllImport("winmm.dll", SetLastError = true)]
	//	//private static extern uint waveInGetNumDevs();

	//	//[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
	//	//private static extern uint waveInGetDevCaps(uint hwo, ref WaveOutCaps pwoc, uint cbwoc);

	//	//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	//	//private struct WaveOutCaps
	//	//{
	//	//	public ushort wMid;
	//	//	public ushort wPid;
	//	//	public uint vDriverVersion;
	//	//	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	//	//	public string szPname;
	//	//	public uint dwFormats;
	//	//	public ushort wChannels;
	//	//	public ushort wReserved1;
	//	//	public uint dwSupport;
	//	//}

	//	public static IEnumerable<string> GetMicrophones()
	//	{
	//		//MMDevice.

	//		var enumerator = new MMDeviceEnumerator();
	//		var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged);
	//		foreach (var device in devices)
	//		{
	//			string name;
	//			try
	//			{
	//				 name = device.FriendlyName;
	//			}
	//			catch (Exception)
	//			{
	//				continue;
	//			}

				
	//			yield return name;
	//		}

	//		//var devicesCount = waveInGetNumDevs();
	//		//var caps = new WaveOutCaps();
	//		//for (uint i = 0; i < devicesCount; i++)
	//		//{
	//		//	waveInGetDevCaps(i, ref caps, (uint)Marshal.SizeOf(caps));

	//		//	yield return caps.szPname;
	//		//}
	//	}
	//}
}
