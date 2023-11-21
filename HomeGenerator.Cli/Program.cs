using System;
using System.Reactive.Linq;
using Mavri.Ha;
using Mavri.Ha.Data;
using Mavri.Ha.Generation;
using SmartHomeDotNet.Hass.Api;
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

public static class HomeAssistantWebSocketApiExtensions
{
	/// <summary>
	/// Gets and observable sequence of the "ios.action_fired" event.
	/// </summary>
	/// <param name="wsApi">The Home-Assistant hub.</param>
	/// <returns>An observable sequence which produces a values each time the event ios.action_fired is raised by Home-Assistant.</returns>
	public static IObservable<EntityStateUpdate> ObserveEntityState(this HomeAssistantWebSocketApi wsApi)
		=> wsApi.Observe("state_changed").Select(evt => evt.GetData<EntityStateUpdate>());

	///// <summary>
	///// Gets and observable sequence of the "ios.action_fired" event.
	///// </summary>
	///// <param name="wsApi">The Home-Assistant hub.</param>
	///// <returns>An observable sequence which produces a values each time the event ios.action_fired is raised by Home-Assistant.</returns>
	//public static IObservable<Timestamped<ZWaveCentralSceneEvent>> ObserveZWaveEvents(this HomeAssistantWebSocketApi wsApi)
	//	=> wsApi.Observe("zwave_js_value_notification").Select(evt => new Timestamped<ZWaveCentralSceneEvent>(evt.GetData<ZWaveCentralSceneEvent>(), evt.Time));
}

public class ComponentEntityAttribute : Attribute
{
	public Component Component { get; }

	public ComponentEntityAttribute(string component)
	{
		Component = component;
	}
}