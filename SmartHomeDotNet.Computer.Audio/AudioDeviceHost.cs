using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices_2;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Computer.Audio
{
	public class AudioDeviceHost : IDeviceHost<AudioDeviceIdentifier, Microphone>, IDeviceHost<AudioDeviceIdentifier, Speaker>
	{
		public static string[] GetDeviceIds()
		{
			return new MMDeviceEnumerator()
				.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active | DeviceState.Unplugged)
				.Select(GetName)
				.Where(name => !string.IsNullOrEmpty(name))
				.ToArray();

			string GetName(MMDevice device)
			{
				try
				{
					return device.FriendlyName;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}

		private ImmutableDictionary<AudioDeviceIdentifier, DeviceListener> _deviceCache = ImmutableDictionary<AudioDeviceIdentifier, DeviceListener>.Empty;

		/// <inheritdoc />
		IObservable<Microphone> IDeviceHost<AudioDeviceIdentifier, Microphone>.Bind(AudioDeviceIdentifier identifier)
			=> Get(identifier)
				.SelectMany(dev =>
				{
					return dev is null
						? Observable.Return(new Microphone("--not found--", identifier.Name, 0, true) {DeviceStatus = DeviceStatus.Unavailable})
						: GetAndObserveVolume(dev.AudioEndpointVolume).Select(volume => new Microphone(dev.ID, dev.FriendlyName, volume.level, volume.muted));
				});

		/// <inheritdoc />
		IObservable<Speaker> IDeviceHost<AudioDeviceIdentifier, Speaker>.Bind(AudioDeviceIdentifier identifier)
			=> Get(identifier)
				.SelectMany(dev =>
				{
					return dev is null
						? Observable.Return(new Speaker("--not found--", identifier.Name, 0, true) {DeviceStatus = DeviceStatus.Unavailable})
						: GetAndObserveVolume(dev.AudioEndpointVolume).Select(volume => new Speaker(dev.ID, dev.FriendlyName, volume.level, volume.muted));
				});

		/// <inheritdoc />
		AsyncContextOperation IDeviceActuator.Execute(ICommand command, params object[] devices)
			=> Execute(command, devices.Cast<AudioDeviceIdentifier>().ToArray());

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, params AudioDeviceIdentifier[] devices)
			=> AsyncContextOperation.StartNew(async ct =>
			{
				foreach (var id in devices)
				{
					var device = Get(id);
					if (device.Current is {} dev)
					{
						switch (command)
						{
							case TurnOn on:
								dev.AudioEndpointVolume.Mute = false;
								break;

							case TurnOff off:
								dev.AudioEndpointVolume.Mute = true;
								break;

							case Toggle toggle:
								dev.AudioEndpointVolume.Mute = !dev.AudioEndpointVolume.Mute;
								break;

							case SetVolume volume:
								dev.AudioEndpointVolume.MasterVolumeLevel = volume.Volume;
								break;
						}
					}
				}
			});

		private DeviceListener Get(AudioDeviceIdentifier identifier)
			=> ImmutableInterlocked.GetOrAdd(ref _deviceCache, identifier, id => new DeviceListener(id));

		private IObservable<(bool muted, float level)> GetAndObserveVolume(AudioEndpointVolume endpoint)
			=> Observable
				.FromEventPattern<AudioEndpointVolumeNotificationDelegate, AudioVolumeNotificationData>(
					h => endpoint.OnVolumeNotification += h,
					h => endpoint.OnVolumeNotification -= h)
				.Select(volume => (volume.EventArgs.Muted, volume.EventArgs.MasterVolume))
				.StartWith((endpoint.Mute, endpoint.MasterVolumeLevel));

		internal class DeviceListener : IMMNotificationClient, IObservable<MMDevice>, IDisposable
		{
			//private static readonly SmartHome.Devices.DeviceState _unavailable = new SmartHome.Devices.DeviceState
			//{
			//	{ "state", "offline" },
			//};

			private readonly Subject<MMDevice?> _device = new Subject<MMDevice?>();
			private MMDevice? _currentDevice = default;

			private readonly MMDeviceEnumerator _enumerator;
			private readonly Func<MMDevice?> _findDevice;

			public MMDevice? Current => _currentDevice;

			public DeviceListener(AudioDeviceIdentifier id)
			{
				_enumerator = new MMDeviceEnumerator();
				_findDevice = FindFromName(_enumerator, id.Name);
				_enumerator.RegisterEndpointNotificationCallback(this);

				UpdateDevice();
			}

			private static Func<MMDevice?> FindFromId(MMDeviceEnumerator enumerator, string id) => ()
				=> enumerator.GetDevice(id);

			private static Func<MMDevice?> FindFromName(MMDeviceEnumerator enumerator, string name) => ()
				=> enumerator
					.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active | DeviceState.Unplugged)
					.FirstOrDefault(dev => dev.FriendlyName == name);

			/// <inheritdoc />
			void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
			{
				if (_currentDevice is { } dev && dev.ID == deviceId)
				{
					UpdateDevice();
				}
			}

			/// <inheritdoc />
			void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
			{
				if (_currentDevice is null)
				{
					UpdateDevice();
				}
			}

			/// <inheritdoc />
			void IMMNotificationClient.OnDeviceRemoved(string deviceId)
			{
				if (_currentDevice is { } dev && dev.ID == deviceId)
				{
					UpdateDevice();
				}
			}

			/// <inheritdoc />
			void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
			{
			}

			/// <inheritdoc />
			void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
			{
			}

			/// <inheritdoc />
			public IDisposable Subscribe(IObserver<MMDevice?> observer)
				=> _device.DistinctUntilChanged().Subscribe(observer);

			private void UpdateDevice()
			{
				_device.OnNext(_currentDevice = _findDevice());
			}

			/// <inheritdoc />
			public void Dispose()
			{
				_enumerator.UnregisterEndpointNotificationCallback(this);
			}
		}
	}
}