using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Mqtt;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet._ThirdParties.Virtual
{
	public class VirtualMqttDevice : IDeviceHost
	{
		private readonly MqttClient _mqtt;
		private readonly string _baseTopic;
		private ImmutableDictionary<string, object> _devices = ImmutableDictionary<string, object>.Empty;

		public VirtualMqttDevice(MqttClient mqtt, string baseTopic, IScheduler scheduler)
		{
			Scheduler = scheduler;
			_mqtt = mqtt;
			_baseTopic = baseTopic;
			//_devices = new HomeDevicesManager<string>(this, new StateProvider(this));
		}

		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public HomeDevice<TDevice> Get<TDevice>(object rawId)
			//where TDevice : new()
		{
			var id = rawId?.ToString();

			var device = ImmutableInterlocked.GetOrAdd(ref _devices, id, Create<TDevice>);
			if (device is HomeDevice<TDevice> typedDevice)
			{
				return typedDevice;
			}
			else
			{
				throw new InvalidCastException($"The device with {id} already exists and is not on the expected type '{typeof(TDevice).Name}' (actual type: {device.GetType().GenericTypeArguments?.FirstOrDefault()?.Name ?? "--unknown--"})");
			}
		}

		private HomeDevice<TDevice> Create<TDevice>(string id)
			//where TDevice : new()
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentOutOfRangeException($"Device id {id} is invalid.");
			}

			var state = _mqtt
				.GetAndObserveTopic(_baseTopic + "/" + id)
				.Select(topic =>
				{
					TDevice device;
					if (string.IsNullOrWhiteSpace(topic.Value))
					{
						device = (TDevice)Activator.CreateInstance(typeof(TDevice));
					}
					else
					{
						device = JsonConvert.DeserializeObject<TDevice>(topic.Value);
					}

					//if (device is IVirtualDevice dev)
					//{
					//	dev.Init(id, this);
					//}

					return (device, topic.IsRetainedState);
				});

			return new HomeDevice<TDevice>(this, id, state, Scheduler);

			//if (typeof(TDevice) == typeof(bool))
			//{
				
			//}
			//else
			//{
			//	throw new ArgumentOutOfRangeException($"Cannot store device of type {typeof(TDevice)} on mqtt");
			//}
		}

		//private class StateProvider : IDeviceStateProvider<string>
		//{
		//	private readonly VirtualMqttDevice _host;

		//	public StateProvider(VirtualMqttDevice host)
		//	{
		//		_host = host;
		//	}

		//	/// <inheritdoc />
		//	public string Parse(object rawId)
		//		=> rawId?.ToString();

		//	/// <inheritdoc />
		//	public IObservable<TDevice> GetAndObserveState<TDevice>(string id)
		//	{
		//		if (typeof(TDevice) == typeof(bool))
		//		{
		//			return _host._mqtt.GetAndObserveTopic(_host._baseTopic + "/" + id).Select()
		//		}
		//		else
		//		{
		//			throw new ArgumentOutOfRangeException($"Cannot store device of type {typeof(TDevice)} on mqtt");
		//		}
		//	}
		//}
	}

	//public interface IVirtualDevice
	//{
	//	void Init(object id, IDeviceHost host);
	//}

	public class VirtualDevice : ILazyDevice
	{
		private int _state = 0;

		/// <inheritdoc />
		[JsonIgnore]
		public object Id { get; private set; }

		/// <inheritdoc />
		[JsonIgnore]
		public IDeviceHost Host { get; private set; }

		/// <inheritdoc />
		public void TryInit(object id, IDeviceHost host)
		{
			id = id ?? throw new NullReferenceException(nameof(id));
			host = host ?? throw new NullReferenceException(nameof(host));

			if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
			{
				Id = id;
				Host = host;
			}
		}
	}

	//public class VirtualSwitch : VirtualDevice, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
	//{
	//	public bool IsOn { get; set; }
	//}

	public abstract class VirtualDevice<TDevice> : HomeDevice<TDevice>
	{
		private readonly IObservable<TDevice> _state;

		public VirtualDevice(MqttClient mqtt, string topic)
			: this(mqtt.GetAndObserveTopic(topic).Select(state => JsonConvert.DeserializeObject<TDevice>(state.Value)))
		{
			Init(new Host(this), topic);
		}

		public VirtualDevice(IObservable<TDevice> state)
		{
			_state = state;
		}

		/// <inheritdoc />
		protected override IObservable<(TDevice value, bool isPersistent)> GetAndObserveState()
			=> _state.Select(state => (state, true));

		protected abstract AsyncContextOperation Execute(ICommand command);

		private class Host : IDeviceHost
		{
			private readonly VirtualDevice<TDevice> _owner;

			public Host(VirtualDevice<TDevice> owner)
			{
				_owner = owner;
			}

			/// <inheritdoc />
			public IScheduler Scheduler { get; }

			/// <inheritdoc />
			public HomeDevice<T> Get<T>(object rawId)
			{
				if (_owner.Id == rawId)
				{
					return (HomeDevice<T>)(object)_owner;
				}
				else
				{
					throw new InvalidOperationException($"Device '{rawId}' is not valid on this host.");
				}
			}

			/// <inheritdoc />
			public AsyncContextOperation Execute(ICommand command, IDevice device)
			{
				if (_owner == device)
				{
					return _owner.Execute(command);
				}
				else
				{
					throw new InvalidOperationException($"Device '{device.Id}' is not valid on this host.");
				}
			}

			/// <inheritdoc />
			public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
				=> Execute(command, devices.Single());
		}
	}

	

	public class VirtualDeviceHost : IDeviceHost
	{
		/// <inheritdoc />
		public IScheduler Scheduler { get; }

		/// <inheritdoc />
		public HomeDevice<TDevice> Get<TDevice>(object rawId)
			=> throw new NotImplementedException();

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IDevice device)
			=> throw new NotImplementedException();

		/// <inheritdoc />
		public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
			=> throw new NotImplementedException();
	}

	public class NightMode<TDevice> : VirtualDevice<TDevice>, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>
	{
		private readonly HomeAssistantApi _ha;
		private readonly MqttClient _mqtt;

		/// <inheritdoc />
		public NightMode(HomeAssistantApi ha, MqttClient mqtt)
			: base(state)
		{
			_ha = ha;
			_mqtt = mqtt;
		}

		/// <inheritdoc />
		protected override AsyncContextOperation Execute(ICommand command)
		{
			switch (command)
			{
				case TurnOn on:
					_ha.Execute("zwave", "set_parameter", new Dictionary<string, string> {{"entity_id", ""}});
					_mqtt.Publish(AsyncContext.CurrentToken, topic, )
			}
		}

		/// <inheritdoc />
		public IDeviceActuator Actuator { get; }
	}

	public class HelloATtribute : Attribute
	{
		public object Id { get; set; }

		public TYPE Type { get; set; }
	}

	public class Test
	{

	}

	//public class VirtualDeviceHost : IDeviceHost
	//{
	//	private ImmutableDictionary<object, IVirtualDevice> _devices = ImmutableDictionary<object, IVirtualDevice>.Empty;

	//	public VirtualDeviceHost(MqttClient mqtt, IScheduler scheduler)
	//	{
	//		Scheduler = scheduler;

	//		_devicesMan = new HomeDevicesManager<object>(this, new DeviceStateProvider());
	//	}

	//	private ImmutableDictionary<object, AsyncDevice<bool>> _booleans = ImmutableDictionary<object, AsyncDevice<bool>>.Empty;
	//	private HomeDevicesManager<object> _devicesMan;

	//	/// <inheritdoc />
	//	public IScheduler Scheduler { get; }

	//	/// <inheritdoc />
	//	public AsyncDevice<TDevice> Get<TDevice>(object rawId)
	//		where TDevice : IDeviceAdapter, new()
	//	{
	//		if (typeof(TDevice) == typeof(bool))
	//		{
	//			return (AsyncDevice<TDevice>)(object)ImmutableInterlocked.GetOrAdd(ref _booleans, rawId, id => new VirtualSwitch(Scheduler));
	//		}
	//		else 
	//		{
	//			throw new InvalidOperationException();
	//		}
	//	}

	//	/// <inheritdoc />
	//	public IDeviceActuator Actuator { get; }

	//	public VirtualDeviceHost Register(IVirtualDevice device)
	//	{
	//		if (device.Id == null)
	//		{
	//			throw new ArgumentException("The device ID is null", nameof(device));
	//		}

	//		if (!ImmutableInterlocked.TryAdd(ref _devices, device.Id, device))
	//		{
	//			throw new InvalidOperationException($"A device with the same id ('{device.Id}') has already been registered.");
	//		}

	//		return this;
	//	}

	//	private IVirtualDevice Get(object id)
	//	{

	//	}

	//	private class DeviceStateProvider : IDeviceStateProvider<object>
	//	{
	//		private readonly VirtualDeviceHost _owner;

	//		public DeviceStateProvider(VirtualDeviceHost owner)
	//		{
	//			_owner = owner;
	//		}

	//		/// <inheritdoc />
	//		public object Parse(object rawId)
	//			=> rawId;

	//		/// <inheritdoc />
	//		public IObservable<DeviceState> GetAndObserveState(object id)
	//			=> _owner.Get(id).GetAndObserveState();
	//	}

	//	private class DeviceActuator : IDeviceActuator
	//	{
	//		private readonly VirtualDeviceHost _owner;

	//		public DeviceActuator(VirtualDeviceHost owner)
	//		{
	//			_owner = owner;
	//		}

	//		/// <inheritdoc />
	//		public AsyncContextOperation Execute(ICommand command, IDevice device)
	//			=> _owner.Get(device.Id).Execute(command);

	//		/// <inheritdoc />
	//		public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
	//			=> AsyncContextOperation.WhenAll(devices.Select(device => _owner.Get(device.Id).Execute(command)));
	//	}

	//	///// <inheritdoc />
	//	//public object GetId(object rawId)
	//	//{
	//	//	if (!_devices.ContainsKey(rawId))
	//	//	{
	//	//		throw new InvalidOperationException($"No device with ID {rawId} has been registered.");
	//	//	}

	//	//	return rawId;
	//	//}

	//	///// <inheritdoc />
	//	//public IObservable<DeviceState> GetAndObserveState(IDevice device)
	//	//	=> GetImpl(device).GetAndObserveState();

	//	///// <inheritdoc />
	//	//public AsyncContextOperation Execute(ICommand command, IDevice device)
	//	//	=> GetImpl(device).Execute(command);

	//	///// <inheritdoc />
	//	//public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
	//	//	=> AsyncContextOperation.WhenAll(devices.Select(device => GetImpl(device).Execute(command)));

	//	//private IVirtualDevice GetImpl(IDevice device)
	//	//	=> _devices.TryGetValue(device.Id, out var implementation)
	//	//		? implementation
	//	//		: throw new InvalidOperationException($"No device with ID {device.Id} has been registered.");
	//}

	

	//public class VirtualSwitch : AsyncDevice<bool>, ISupport<TurnOn>, ISupport<TurnOff>, ISupport<Toggle>, IVirtualDevice
	//{
	//	private readonly MqttClient _mqtt;

	//	/// <inheritdoc />
	//	public VirtualDevice(object id, MqttClient mqtt, IScheduler scheduler)
	//		: base(host, id, source, toDevice, scheduler)
	//	{
	//		_mqtt = mqtt;

	//		var src = mqtt.GetAndObserveTopic("bla").Select(topic => new Device())
	//	}

	//	/// <inheritdoc />
	//	public override object Id { get; }

	//	/// <inheritdoc />
	//	public override IDeviceHost Host { get; }

	//	/// <inheritdoc />
	//	protected override bool HasState { get; }

	//	/// <inheritdoc />
	//	protected override bool State { get; }

	//	/// <inheritdoc />
	//	protected override TaskAwaiter GetAwaiterImpl()
	//		=> throw new NotImplementedException();

	//	/// <inheritdoc />
	//	public override void Dispose()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <inheritdoc />
	//	public IDeviceActuator Actuator { get; set; }

	//	/// <inheritdoc />
	//	public AsyncContextOperation Execute(ICommand command)
	//	{
	//		switch (command)
	//		{
	//			case TurnOn on:
	//				return AsyncContextOperation.StartNew(ct => _mqtt.Publish(ct, "blabla", "on", QualityOfService.AtLeastOnce, retain: true);
	//			case TurnOff off:
	//				return AsyncContextOperation.StartNew(ct => _mqtt.Publish(ct, "blabla", "off", QualityOfService.AtLeastOnce, retain: true);
	//			case TurnOn on:
	//				return AsyncContextOperation.StartNew(ct => _mqtt.Publish(ct, "blabla", "on", QualityOfService.AtLeastOnce, retain: true);
	//		}
	//	}
	//}

	//public interface IVirtualDevice
	//{
	//	AsyncContextOperation Execute(ICommand command);
	//}



	//public class VirtualDeviceHost : IDeviceHost
	//{
	//	private ImmutableDictionary<object, IVirtualDevice> _devices = ImmutableDictionary<object, IVirtualDevice>.Empty;

	//	public VirtualDeviceHost(IScheduler scheduler)
	//	{
	//		Scheduler = scheduler;
	//	}

	//	/// <inheritdoc />
	//	public IScheduler Scheduler { get; }

	//	public VirtualDeviceHost Register(IVirtualDevice device)
	//	{
	//		if (device.Id == null)
	//		{
	//			throw new ArgumentException("The device ID is null", nameof(device));
	//		}

	//		if (!ImmutableInterlocked.TryAdd(ref _devices, device.Id, device))
	//		{
	//			throw new InvalidOperationException($"A device with the same id ('{device.Id}') has already been registered.");
	//		}

	//		return this;
	//	}

	//	/// <inheritdoc />
	//	public object GetId(object rawId)
	//	{
	//		if (!_devices.ContainsKey(rawId))
	//		{
	//			throw new InvalidOperationException($"No device with ID {rawId} has been registered.");
	//		}

	//		return rawId;
	//	}

	//	/// <inheritdoc />
	//	public IObservable<DeviceState> GetAndObserveState(IDevice device)
	//		=> GetImpl(device).GetAndObserveState();

	//	/// <inheritdoc />
	//	public AsyncContextOperation Execute(ICommand command, IDevice device)
	//		=> GetImpl(device).Execute(command);

	//	/// <inheritdoc />
	//	public AsyncContextOperation Execute(ICommand command, IEnumerable<IDevice> devices)
	//		=> AsyncContextOperation.WhenAll(devices.Select(device => GetImpl(device).Execute(command)));

	//	private IVirtualDevice GetImpl(IDevice device)
	//		=> _devices.TryGetValue(device.Id, out var implementation) 
	//			? implementation 
	//			: throw new InvalidOperationException($"No device with ID {device.Id} has been registered.");
	//}

	//public interface IVirtualDevice
	//{
	//	object Id { get; }

	//	IObservable<DeviceState> GetAndObserveState();

	//	AsyncContextOperation Execute(ICommand command);
	//}
}
