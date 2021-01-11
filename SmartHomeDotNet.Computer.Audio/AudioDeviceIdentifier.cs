using System;
using System.Linq;

namespace SmartHomeDotNet.Computer.Audio
{
	public readonly struct AudioDeviceIdentifier
	{
		public AudioDeviceIdentifier(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}