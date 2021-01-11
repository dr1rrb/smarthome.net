using System;
using System.Linq;
using System.Reactive.Concurrency;
using SmartHomeDotNet.Computer.Audio;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices_2;

namespace SmartHomeDotNet.Computer.LocalTests
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Welcome to the dev console!");

			var devices = AudioDeviceHost.GetDeviceIds();
			
			var host = new AudioDeviceHost();
			var registry = new DeviceRegistry(Scheduler.Immediate);

			registry.RegisterHost(host);

			var dev = registry.Bind<Microphone>(new AudioDeviceIdentifier(devices.First()));
			//var dev = registry.Bind<Microphone>(new AudioDeviceIdentifier("Microphone (2- Logitech G933 Gaming Headset)"));
			//var dev = registry.Bind<Microphone>(new AudioDeviceIdentifier("Casque (PXC 550 Stereo)"));

			dev.TurnOff().ToTask().Wait();
		}
	}
}
