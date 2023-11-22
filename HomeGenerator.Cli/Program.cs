using System;
using Mavri.Ha;
using Mavri.Ha.Generation;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.Utils;
using Component = SmartHomeDotNet.Hass.Component;
using IDevice = SmartHomeDotNet.SmartHome.Devices.IDevice;

namespace HomeGenerator.Cli;

internal class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");

		var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (snd, e) => cts.Cancel(true);


		//Task.Run(async () =>
		//{
		//	var home = new Home();

		//	await Task.Delay(60_000);

		//}, cts.Token).Wait(cts.Token);


		//Task.Run(async () =>
		//{
		//	var haApi = new HomeAssistantWebSocketApi();
		//	var hub = new HomeAssistantActuator(haApi);
		//	var entities = new GeykHome.EntityRegistry(hub);
		//	var devs = new GeykHome.DeviceRegistry(entities);
		//	var geyk = new GeykHome.Home(devs);

		//	//var light = await geyk.Bureau.PlafonnierDuBureau.Light;
		//	//geyk.SousSol.ControleurVentilateursRigs_f906587ea03fbf36bdf81b0c1ede7fc7


		//}, cts.Token).Wait(cts.Token);
	}
}

public class ComponentEntityAttribute : Attribute
{
	public Component Component { get; }

	public ComponentEntityAttribute(string component)
	{
		Component = component;
	}
}