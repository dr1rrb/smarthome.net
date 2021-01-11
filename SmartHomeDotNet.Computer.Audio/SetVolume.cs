using System;
using System.Linq;
using SmartHomeDotNet.SmartHome.Commands;

namespace SmartHomeDotNet.Computer.Audio
{
	public struct SetVolume : ICommand
	{
		public float Volume { get; }

		public SetVolume(float volume)
		{
			Volume = volume;
		}
	}
}