# smarthome.net
This project is designed to let you interact with all you smart devices to build rich rules for your smart home using C#. 
Currently we support the following integration, but you can create [your own](./doc/extend.md)
* Home Assistant
* GPIO (with Windows IoT Core, which can be used with a Raspberry Pi for instance)
* Zigbee2MQTT (Which allows you to react faster to events by avoiding the cost of HA processing)
* MQTT

## Builds
[![Build Status](https://dev.azure.com/dr1rrb/smarthome.net/_apis/build/status/dr1rrb.smarthome.net?branchName=master)](https://dev.azure.com/dr1rrb/smarthome.net/_build/latest?definitionId=2&branchName=master)
[![nuget (stable)](https://img.shields.io/nuget/v/smarthomedotnet.svg)](https://www.nuget.org/packages/smarthomedotnet/)
[![nuget (dev)](https://img.shields.io/nuget/vpre/smarthomedotnet.svg)](https://www.nuget.org/packages/smarthomedotnet/)

## Main concepts
* **Automation**: This is the way to run some code to react to a trigger, like a device state change or just on a schedule basis.
* **Scene**: This is an asynchronous sequence of task to acheive. It can be trigger by an automation, a switch in Home-Assistant, or by code ...

## How to use it

### Configuration
The first thing to do is to configure your "Home".

_Currently this is quite verbose, we are working on it, stay tuned !_

```csharp
public class GeyksHome : HomeBase<GeyksHome>
{
	/// <inheritdoc />
	protected override HomeDevicesManager DefaultDeviceManager => HA.Devices;

	/// <inheritdoc />
	protected override ISceneHost DefaultSceneHost => HA.Scenes;

	/// <inheritdoc />
	protected override IAutomationHost DefaultAutomationHost => HA.Automations;

	/// <inheritdoc />
	protected override IEnumerable<IDisposable> CreateHubs()
	{
		var homeTopic = "geykshome";
		var mqttBroker = new MqttBrokerConfig
		{
			Host = "192.168.144.207",
			ClientStatusTopic = homeTopic + "/state"
		};
		var mqtt = new MqttClient(mqttBroker, Scheduler, HomeAssistantHub.DefaultTopic, Zigbee2MqttHub.DefaultTopic, homeTopic);

		yield return HA = new HomeAssistantHub(Scheduler, "192.168.144.207:8123", "*****", mqtt, homeTopic: homeTopic);
		yield return Zigbee = new Zigbee2MqttHub(mqtt, Scheduler);
		yield return ST = new SmartThings(mqtt);
	}

	/// <inheritdoc />
	protected override void CreateRooms()
	{
		// Here you create your home areas. 
		// They are expected to inherit from Room<GeyksHome>
	
		Kitchen = new Kitchen();
		LivingRoom = new LivingRoom();
		BedRoom = new BedRoom();
		Entrance = new Entrance();
		Office = new Office();
		DiningRoom = new DiningRoom();
		Bathroom1 = new Bathroom1();
		Bathroom2 = new Bathroom2();
	}

	/// <inheritdoc />
	protected override IEnumerable<IDisposable> CreateScenes()
	{
		yield return Cinema = new Cinema();
		yield return GoingToSleep = new GoingToSleep();
		yield return GoodBye = new GoodBye();
		yield return GoodMorning = new GoodMorning();
		yield return GoodNight = new GoodNight();
		yield return PartyTime = new PartyTime();
		yield return WelcomeBack = new WelcomeBack();
		yield return VentilateBathroom2 = new VentilateBathroom2();
	}

	/// <inheritdoc />
	protected override IEnumerable<IDisposable> CreateAutomations()
	{
		// Buttons
		yield return new CubeActions();
		yield return new BedroomButtonsActions();
		yield return new Bathroom1ButtonActions();

		// Lightning
		yield return new KitchenLights();

		// ScheduledRoutines
		yield return new AutoGoingToSleep();
		yield return new AutoVentilateBathroom2();

		// Misc
		yield return new SmartThingsRoutines();
	}

	// Hubs
	public HomeAssistantHub HA { get; private set; }
	public Zigbee2MqttHub Zigbee { get; set; }
	public SmartThings ST { get; set; }

	// Scenes
	public Cinema Cinema { get; private set; }
	public GoingToSleep GoingToSleep { get; private set; }
	public GoodBye GoodBye { get; private set; }
	public GoodMorning GoodMorning { get; private set; }
	public GoodNight GoodNight { get; private set; }
	public PartyTime PartyTime { get; private set; }
	public WelcomeBack WelcomeBack { get; private set; }
	public VentilateBathroom2 VentilateBathroom2 { get; private set; }

	// Rooms
	public Bathroom1 Bathroom1 { get; set; }
	public Bathroom2 Bathroom2 { get; set; }
	public Kitchen Kitchen { get; private set; }
	public LivingRoom LivingRoom { get; private set; }
	public BedRoom BedRoom { get; private set; }
	public Entrance Entrance { get; private set; }
	public Office Office { get; private set; }
	public DiningRoom DiningRoom { get; private set; }
}
```

Example of a room:

```csharp
public class Bathroom2 : Room<GeyksHome>
{
	public HomeDevice<Fan> Vent { get; } = Get<Fan>("fan.bathroom_2_ventilation");

	public HomeDevice<Switch> Perfume { get; } = Get<Switch>("switch.bathroom_2_parfum");
}
```

### Scene
Those are just a sequence of task.

Here an example designed to prepare your eyes and mind to the night: It will slowly fade on all RGB light to RED, 
while turning off all others light using some specific timming.

```csharp
public class GoingToSleep : Scene<GeyksHome>
{
	/// <inheritdoc />
	public GoingToSleep()
		: base("going_to_sleep", "Going to sleep")
	{
	}

	/// <inheritdoc />
	protected override async Task Run()
	{
		await Home.HA.Api.Select(HomeMode.GoingToSleep, Home.Mode);

		Home.HA.Api.TurnOn(
			.75, Color.Red, TimeSpan.FromSeconds(15),
			Home.Office.Light,
			Home.BedRoom.Light,
			Home.LivingRoom.TopLight);

		await AsyncContext.Delay(TimeSpan.FromSeconds(10));

		// Kitchen lights does not supports the fade delay so turn it on AFTER the delay (ST ?)
		Home.HA.Api.TurnOn(
			.05, TimeSpan.FromSeconds(15),
			Home.Kitchen.TopLight);

		Home.HA.Api.TurnOff(
			TimeSpan.FromSeconds(5),
			Home.Kitchen.CounterLight,
			Home.Entrance.Light,
			Home.DiningRoom.Light,
			Home.LivingRoom.PlantsLight);

		Home.HA.Api.TurnOn(Home.Kitchen.CounterLight_NightMode);
		if ((await Home.Bathroom1.Light).IsOn)
		{
			await Home.HA.Api.TurnOn(.05, TimeSpan.FromSeconds(30), Home.Bathroom1.Light);
		}
		else
		{
			await Home.HA.Api.TurnOn(.05, Home.Bathroom1.Light);
			await Home.HA.Api.TurnOff(Home.Bathroom1.Light);
		}
	}
}
```

### Automate
A common case is to automatically start a scens at a given time of the day
```csharp
public class AutoVentilateBathroom2 : Automation<GeyksHome>
{
	public AutoVentilateBathroom2() : base("ventilate_bathroom_2", "Auto ventilate bathroom #2")
	{
	}

	protected override IDisposable Enable()
		=> Observable
			.Timer(Scheduler.Now.Date + new TimeSpan(10, 00, 00), TimeSpan.FromHours(24), Scheduler)
			.Where(_ => Scheduler.Now.TimeOfDay < new TimeSpan(10, 00, 30) && Scheduler.Now.DayOfWeek != DayOfWeek.Saturday && Scheduler.Now.DayOfWeek != DayOfWeek.Sunday)
			.Do(_ => Home.VentilateBathroom2.Start())
			.SubscribeWithContext();
}
```


Another common case is to react to a device state changed.

Here an example which will customize the response depending of the time of day and the current state of devices:
```csharp
public class BedroomButtonsActions : Automation<GeyksHome>
{
	public BedroomButtonsActions() 
		: base("buttons_bedroom", "Bedroom buttons")
	{
	}

	protected override IDisposable Enable()
		=> new CompositeDisposable
		{
			Home.BedRoom.DavidButton.Where(b => b.Action == Button.Actions.Single).SubscribeWithContext(OnSingleTap),
			Home.BedRoom.DavidButton.Where(b => b.Action == Button.Actions.Double).SubscribeWithContext(OnDoubleTap),
			Home.BedRoom.MaelButton.Where(b => b.Action == Button.Actions.Single).SubscribeWithContext(OnSingleTap),
			Home.BedRoom.MaelButton.Where(b => b.Action == Button.Actions.Double).SubscribeWithContext(OnDoubleTap),
		};

	private async Task OnSingleTap(Button button)
	{
		var timeOfDay = DateTimeOffset.Now.LocalDateTime.TimeOfDay;
		var isMorning = timeOfDay > TimeSpan.FromHours(4) && timeOfDay < TimeSpan.FromHours(12);

		if (await Home.Mode == HomeMode.GoingToSleep)
		{
			Home.GoodNight.Start();
		}
		else if (isMorning)
		{
			if ((await Home.BedRoom.Light).IsOn)
			{
				await Home.HA.Api.TurnOff(Home.BedRoom.Light);
			}
			else
			{
				Home.GoodMorning.Start();
			}
		}
		else
		{
			if ((await Home.BedRoom.Light).IsOn)
			{
				Home.GoodNight.Start();
			}
			else
			{
				await Home.HA.Api.TurnOn(.75, Color.Red, Home.BedRoom.Light);
			}
		}
	}

	private async Task OnDoubleTap(Button button)
	{
		var light = await Home.BedRoom.Light;
		if (light.IsOn)
		{
			Home.HA.Api.TurnOff(light);
		}
		else
		{
			Home.HA.Api.TurnOn(1, Color.Bisque, light);
		}
	}
}
```

### Test your home
It's **really easy** to debug your scenes and automations!

1. Create a new console application
1. In your main method, add the following:
	```csharp
	using System;

	namespace GeykHome.LiveTests
	{
		class Program
		{
			static void Main(string[] args)
			{
				using (var home = new GeyksHome())
				{
					home.MakeItSmart();

					Console.WriteLine("Press enter to exit.");
					Console.ReadLine();
				}
			}
		}
	}
	```
1. And that's it! Hit `F5` and start debug your smart home using real devices and the rich debugger of Visuaal studio!

Note: If you already have a running instance of _smarthhome.net_, you have to manually stop it before starting debug,
otherwise both instances will react to device triggers, which may result into strange behaviors!

### Deploy
You can deploy your application directly directly in a command line tool that you run on a device (Linux, Windows, etc.), 
or even package it as a Docker container based on the `microsoft/dotnet:2.1-aspnetcore-runtime`

_We are planning to develop an integration using hass.io_
