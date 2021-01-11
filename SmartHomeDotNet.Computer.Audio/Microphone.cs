#nullable enable

using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace SmartHomeDotNet.Computer.Audio
{
	public class Microphone : Device, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<SetVolume>, IDeviceState
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
}
