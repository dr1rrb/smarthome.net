using System;
using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using HomeGenerator.Cli.Generation;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.Hass.Events;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Devices_2;
using SmartHomeDotNet.Utils;
using static HomeGenerator.Cli.Generation.Generator;
using Component = SmartHomeDotNet.Hass.Component;

namespace HomeGenerator.Cli
{
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

	internal sealed class EntitiesManager : IDisposable
	{
		private readonly IEntityRegistry _entities;
		private readonly HomeAssistantWebSocketApi _api;
		private readonly IDisposable _subscription;
		private readonly CancellationTokenSource _ct = new();

		public EntitiesManager(IEntityRegistry entities, HomeAssistantWebSocketApi api)
		{
			_entities = entities;
			_api = api;

			_subscription = api.ObserveEntityState().Subscribe(update => Publish(update.NewState));
			_ = LoadInitialStates(_ct.Token);
		}

		private async Task LoadInitialStates(CancellationToken ct)
		{
			try
			{
				var states = await _api.Send<ImmutableArray<EntityState>>(new GetStatesCommand(), ct);
				foreach (var state in states)
				{
					Publish(state);
				}
			}
			catch (Exception)
			{
				this.Log().Error("Failed to load initial states.");
			}
		}

		private void Publish(EntityState state)
		{
			try
			{
				_entities.Get(state.EntityId)?.Publish(state);
			}
			catch (Exception)
			{
				this.Log().Error($"Failed to publish new state for entity '{state.EntityId}'.");
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_subscription.Dispose();
			_ct.Cancel();
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

	public record EntityStateUpdate(string EntityId, EntityState OldState, EntityState NewState);

	public interface IEntityRegistry
	{
		IEntity? Get(EntityId id);
	}

	public interface IDeviceRegistry
	{
		IDevice? Get(DeviceId id);
	}

	public interface IEntity : IThingInfo<EntityId>
	{
		void Publish(EntityState state);
	}

	public interface IDevice : IThingInfo<DeviceId>
	{
	}

	public class ComponentEntityAttribute : Attribute
	{
		public Component Component { get; }

		public ComponentEntityAttribute(string component)
		{
			Component = component;
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class EntityAttribute : Attribute
	{
		public EntityId EntityId { get; }
		
		/// <summary>
		/// The type of the <see cref="Entity{T}"/> to use to back that given entity
		/// </summary>
		public Type EntityType { get; }

		public EntityAttribute(string entityId, Type entityType)
		{
			EntityId = entityId;
			EntityType = entityType;
		}
	}

	//public sealed record BooleanEntity(EntityId Id, IDeviceActuator<EntityId> Actuator) : Entity<bool>(Id, Actuator)
	//{
	//	/// <inheritdoc />
	//	protected override bool Parse(EntityState raw)
	//		=> ;
	//}

	public interface IHomeAssistantHub : IDeviceActuator<EntityId>, IDeviceActuator<DeviceId>
	{
		/// <summary>
		/// The base (http[s]) uri of this home-assistant instance.
		/// </summary>
		public Uri BaseUri { get; }

		/// <summary>
		/// Registry of entities hosted by this home-assistant instance.
		/// </summary>
		public IEntityRegistry Entities { get; }

		/// <summary>
		/// Registry of devices hosted by this home-assistant instance.
		/// </summary>
		public IDeviceRegistry Devices { get; }
	}

	public class HomeAssistantActuator : IDeviceActuator<EntityId>, IDeviceActuator<DeviceId>
	{
		public HomeAssistantActuator(HomeAssistantWebSocketApi api)
		{
			
		}

		/// <inheritdoc />
		AsyncContextOperation IDeviceActuator.Execute(ICommand command, params object[] devices)
			=> throw new NotImplementedException();

		/// <inheritdoc />
		AsyncContextOperation IDeviceActuator<EntityId>.Execute(ICommand command, params EntityId[] devices)
			=> throw new NotImplementedException();

		/// <inheritdoc />
		AsyncContextOperation IDeviceActuator<DeviceId>.Execute(ICommand command, params DeviceId[] devices)
			=> throw new NotImplementedException();
	}

	public interface IHomeAssistantDevice
	{
		/// <summary>
		/// Gets the main entity of this device.
		/// </summary>
		EntityId Main { get; }
	}

	//public record Entity(EntityId Id) : IObservable<EntityState>
	//{
	//	private readonly BehaviorSubject<EntityState?> _state = new(default);

	//	public void Publish(EntityState state)
	//		=> _state.OnNext(state);

	//	/// <inheritdoc />
	//	public IDisposable Subscribe(IObserver<EntityState> observer)
	//		=> _state.Where(state => state is not null).Cast<EntityState>().Subscribe(observer);
	//}

	//public abstract record Device<TMain>(DeviceId Id, Entity<TMain> Main)
	//	where TMain : notnull;

	public abstract record Area(AreaId Id);

	// POUR LE MONE T ON SE LIMITE A UTILISER LES ENTITY (Qui agrégent déjà les attributs qu'on recevait 1 à 1)
	public abstract record Device(DeviceId Id, IHomeAssistantHub Hub) : IDevice
	{
		IDeviceActuator<DeviceId> IThingInfo<DeviceId>.Actuator => Hub;
	}
	//public abstract record Device<TState>(DeviceId Id, IDeviceActuator<DeviceId> Actuator) : IThing<DeviceId, TState?>
	//	where TState : notnull, new()
	//{
	//	private readonly CompositeDisposable _subscriptions = new();
	//	private readonly Subject<TState> _state = new();

	//	private TState _current = new();
	//	private bool _isCurrentValid;

	//	protected void Register<T>(Entity<T> entity, Func<TState, T?, TState> update, bool isMain = false)
	//		where T : notnull
	//	{
	//		entity
	//			.State
	//			.Synchronize(_state)
	//			.Subscribe(state =>
	//			{
	//				if (isMain)
	//				{
	//					_isCurrentValid = state is { IsUnavailable: false, IsUnknown: false };
	//				}

	//				_current = update(_current, state.Value);

	//				if (_isCurrentValid)
	//				{
	//					_state.OnNext(_current);
	//				}
	//			})
	//			.DisposeWith(_subscriptions);
	//	}

	//	/// <inheritdoc />
	//	public IDisposable Subscribe(IObserver<TState?> observer)
	//		=> _state.StartWith(_current).Throttle(TimeSpan.FromMilliseconds(50)).Subscribe(observer);
	//}

	//public record IKEATrucChoseDevice(DeviceId Id, ButtonEntity Button) : Device(Id);

	//public sealed record IKEATrucChose(DeviceId Id, IDeviceActuator<DeviceId> Actuator, Entity<bool> IsOn, Entity<DateTimeOffset> LastSeen) : Device(Id);//, ISupport<TurnOn>;

	//public record struct IKEATRucChoseState(bool IsOn, DateTimeOffset? LastSeen);


	internal record Config(
		[property: JsonPropertyName("location_name")] string? Name);


	/*
		{
			"aliases": [],
			"area_id": "950f5d119c0e4967b9ba243dae5c54e3",
			"name": "Chambre",
			"picture": null
		}
	 */
	internal record AreaInfo(
		[property: JsonPropertyName("area_id")] string Id,
		string? Name = null);

	/*
		{
			"area_id": "950f5d119c0e4967b9ba243dae5c54e3",
			"configuration_url": null,
			"config_entries": ["fb59f1e0999016c68b5786a52ca07fff"],
			"connections": [],
			"disabled_by": null,
			"entry_type": null,
			"hw_version": null,
			"id": "6ca196c5a220b6094704091a490f3b87",
			"identifiers": [["zwave_js", "3277824893-21"], ["zwave_js", "3277824893-21-798:5:1"]],
			"manufacturer": "Inovelli",
			"model": "LZW42",
			"name_by_user": "Globe de la chambre",
			"name": "inovelli_lzw42_001",
			"serial_number": null,
			"sw_version": "2.28",
			"via_device_id": "5b881e3ffafaeb54f06ae10531873786"
		}
	 */
	internal record DeviceInfo(
		string Id, 
		string Name,
		[property:JsonPropertyName("name_by_user")] string? FriendlyName = null, 
		string? AreaId = null,
		string? Model = null,
		string? Manufacturer = null);


	/*
		{
			"area_id": null,
			"config_entry_id": "6923d162b036f7e4e8803283be419948",
			"device_id": "f377941666bb27d80ac35ee23f5325db",
			"disabled_by": null,
			"entity_category": "diagnostic",
			"entity_id": "sensor.workbench_light_node_status",
			"has_entity_name": true,
			"hidden_by": null,
			"icon": null,
			"id": "103b30726f96738d52f2396deb2389c8",
			"name": "Lumière établis: Node Status",
			"options": {
				"conversation": {
					"should_expose": false
				}
			},
			"original_name": "Node status",
			"platform": "zwave_js",
			"translation_key": null,
			"unique_id": "4059427701.15.node_status"
		}
	 */
	internal record EntityInfo(
		[property: JsonPropertyName("entity_id")] string RawId,
		[property: JsonPropertyName("has_entity_name")] bool HasName,
		string? Name = null,
		string? OriginalName = null,
		string? AreaId = null,
		string? DeviceId = null)
	{
		[JsonIgnore]
		public EntityId Id { get; } = EntityId.Parse(RawId);
	}

	/*
		{
			"entity_id": "sensor.hydroqc_1508_bassins_yesterday_morning_peak_saved_consumtion",
			"state": "unavailable",
			"attributes": {
				"unit_of_measurement": "kWh",
				"device_class": "energy",
				"icon": "mdi:home-lightning-bolt",
				"friendly_name": "Hydro Québec Yesterday morning peak saved consumtion"
			},
			"last_changed": "2023-11-04T00:43:20.152984+00:00",
			"last_updated": "2023-11-04T00:43:20.152984+00:00",
			"context": {
				"id": "01HEBWHJ6R1E0VZVVQCPWPXTBP",
				"parent_id": null,
				"user_id": null
			}
		}
	 */
	public record EntityState(
		[property: JsonPropertyName("entity_id")] string RawEntityId,
		string State,
		EntityAttributeCollection Attributes,
		DateTimeOffset LastChanged,
		DateTimeOffset LastUpdated)
	{
		public const string Unavailable = "unavailable";
		public const string Unknown = "unknown";

		[JsonIgnore]
		public EntityId EntityId { get; } = EntityId.Parse(RawEntityId);

		[JsonIgnore]
		public bool IsUnavailable => State is Unavailable;
		
		[JsonIgnore]
		public bool IsUnknown => State is Unknown;
	}

	[JsonConverter(typeof(EntityAttributeCollectionJsonConverter))]
	public record EntityAttributeCollection
	{
		//private readonly IMemoryOwner<byte> _json;
		private readonly JsonObject _node;

		private readonly JsonSerializerOptions _options;
		//private readonly JsonReaderOptions _readOptions;
		//private readonly JsonSerializerOptions _options;

		//private ImmutableDictionary<string, Attribute>? _values;

		public EntityAttributeCollection(JsonObject node, JsonSerializerOptions options)
		{
			_node = node;
			_options = options;
		}

		//public EntityAttributeCollection(IMemoryOwner<byte> json, JsonReaderOptions readOptions, JsonSerializerOptions options)
		//{
		//	_json = json;
		//	_readOptions = readOptions;
		//	_options = options;
		//}

		//[MemberNotNull(nameof(_values))]
		//private void Populate()
		//{
		//	if (_values is not null)
		//	{
		//		return;
		//	}

		//	ImmutableDictionary<string, Attribute> values;
		//	var reader = new Utf8JsonReader(_json.Memory.Span, _readOptions);
		//	if (!reader.Read() || reader.TokenType is not JsonTokenType.StartObject)
		//	{
		//		values = ImmutableDictionary<string, Attribute>.Empty;
		//	}
		//	else
		//	{
		//		var builder = ImmutableDictionary.CreateBuilder<string, Attribute>();
		//		while (ReadToNotComment(reader) && reader.TokenType is JsonTokenType.PropertyName)
		//		{
		//			var key = reader.GetString();
		//			if (key is null || !reader.Read())
		//			{
		//				break; // Invalid json
		//			}

		//			var value = ReadValue(reader);
		//			if (value is null)
		//			{
		//				break; // Invalid json
		//			}

		//			builder.Add(key, value);
		//		}
		//		values = builder.ToImmutable();

		//		Debug.Assert(ReadToNotComment(reader));
		//	}

		//	Interlocked.CompareExchange(ref _values, values, null);
		//}

		//private static bool ReadToNotComment(Utf8JsonReader reader)
		//	=> reader.Read() && (reader.TokenType is not JsonTokenType.Comment || ReadToNotComment(reader));

		//private static Attribute? ReadValue(Utf8JsonReader reader)
		//{
		//	switch (reader.TokenType)
		//	{
		//		case JsonTokenType.StartObject:
		//		case JsonTokenType.StartArray:
		//			var start = (int)reader.TokenStartIndex;
		//			reader.Skip();
		//			var length = (int)reader.TokenStartIndex - 1 - start;
		//			return new Attribute(false, null, (start, length));

		//		case JsonTokenType.Null:
		//		case JsonTokenType.None:
		//			return Attribute.Null;

		//		case JsonTokenType.Comment:
		//			reader.Read();
		//			return ReadValue(reader);

		//		case JsonTokenType.Number:
		//			return new Attribute(true, reader.GetDouble(), null);

		//		case JsonTokenType.True:
		//			return new Attribute(true, true, null);

		//		case JsonTokenType.False:
		//			return new Attribute(true, false, null);

		//		case JsonTokenType.String:
		//			return new Attribute(true, reader.GetString(), null);

		//		case JsonTokenType.EndObject:
		//		case JsonTokenType.EndArray:
		//		case JsonTokenType.PropertyName:
		//		default:
		//			return null;
		//	}
		//}

		//private record Attribute(bool HasValue, object? Value, (int start, int length)? Json)
		//{
		//	public static Attribute Null { get; } = new(false, null, null);
		//}

		//public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
		//{
		//	Populate();

		//	if (!_values.TryGetValue(key, out var attribute))
		//	{
		//		value = default;
		//		return false;
		//	}

		//	if (attribute.HasValue && Convert.ChangeType(attribute.Value, typeof(T), CultureInfo.InvariantCulture) is { } t)
		//	{
		//		value = (T)t;
		//		return true;
		//	}

		//	if (attribute.Json is { } json)
		//	{
		//		// TODO: Cache ?
		//		value = JsonSerializer.Deserialize<T>(_json.Memory.Span.Slice(json.start, json.length), _options);
		//		return value is not null;
		//	}

		//	value = default;
		//	return false;
		//}

		public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
		{
			if (!_node.TryGetPropertyValue(key, out var valueNode) || valueNode is null)
			{
				value = default;
				return false;
			}

			if (valueNode is JsonValue va)
			{
				if (typeof(T).IsAssignableTo(typeof(Enum)) && va.TryGetValue(out string? rawValue))
				{
					if (Enum.TryParse(typeof(T), ToCSharpCamel(rawValue), ignoreCase: true, out var enumValue))
					{
						value = (T)enumValue;
						return true;
					}
					else
					{
						value = default;
						return false;
					}
				}
				else
				{
					return va.TryGetValue(out value);
				}
			}
			else
			{
				value = valueNode.Deserialize<T>(_options);
				return value is not null;
			}

			//if (!_values.TryGetValue(key, out var attribute))
			//{
			//	value = default;
			//	return false;
			//}

			//if (attribute.HasValue && Convert.ChangeType(attribute.Value, typeof(T), CultureInfo.InvariantCulture) is { } t)
			//{
			//	value = (T)t;
			//	return true;
			//}

			//if (attribute.Json is { } json)
			//{
			//	// TODO: Cache ?
			//	value = JsonSerializer.Deserialize<T>(_json.Memory.Span.Slice(json.start, json.length), _options);
			//	return value is not null;
			//}

			//value = default;
			//return false;
		}

		//public bool IsSet(string key)
		//{
		//	Populate();

		//	if (!_values.TryGetValue(key, out var attribute))
		//	{
		//		return false;
		//	}

		//	return attribute.HasValue || attribute.Json is not null;
		//}

		public bool IsSet(string key) 
			=> _node.ContainsKey(key);

		private void Set<T>(string key, T value)
		{
			throw new NotImplementedException();
		}
	}

	public static class EntityAttributeCollectionExtensions
	{
		public static T Get<T>(this EntityAttributeCollection attributes, string key, EntityId id)
			=> attributes.TryGet(key, out T? value) ? value : throw new InvalidOperationException($"Attribute {key} is missing for entity '{id}'.");

		public static T GetOrDefault<T>(this EntityAttributeCollection attributes, string key, T defaultValue)
			=> attributes.TryGet(key, out T? value) ? value : defaultValue;

		public static ImmutableArray<T> GetArray<T>(this EntityAttributeCollection attributes, string key, EntityId id)
			=> attributes.TryGet(key, out ImmutableArray<T> value) ? value : throw new InvalidOperationException($"Attribute {key} is missing for entity '{id}'.");

		public static ImmutableArray<T> GetArrayOrDefault<T>(this EntityAttributeCollection attributes, string key)
			=> attributes.TryGet(key, out ImmutableArray<T> value) ? value : ImmutableArray<T>.Empty;
	}

	public static class EntityStatenExtensions
	{
		public static T GetState<T>(this EntityState state, EntityId id)
			where T : struct, Enum
			=> Enum.TryParse(ToCSharpCamel(state.State), ignoreCase: true, out T value) ? value : throw new InvalidOperationException($"Unknown state '{state.State}' for entity '{id}'.");

		public static bool GetOnOffState(this EntityState state, EntityId id)
			=> state.State.ToLowerInvariant() switch
			{
				"on" => true,
				"off" => false,
				_ => throw new InvalidOperationException($"Unknown state '{state.State}' for a entity '{id}'.")
			};
	}

	public class EntityAttributeCollectionJsonConverter : JsonConverter<EntityAttributeCollection>
	{
		/// <inheritdoc />
		public override EntityAttributeCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			//var json = MemoryPool<byte>.Shared.Rent();
			//try
			//{
			//	if (reader.HasValueSequence)
			//	{
			//		reader.ValueSequence.CopyTo(json.Memory.Span);
			//	}
			//	else
			//	{
			//		reader.ValueSpan.CopyTo(json.Memory.Span);
			//	}
			//	reader.Skip();
			//	return new EntityAttributeCollection(json, reader.CurrentState.Options, options);
			//}
			//catch (Exception)
			//{
			//	json.Dispose();
			//	throw;
			//}
			return JsonNode.Parse(ref reader, new JsonNodeOptions { PropertyNameCaseInsensitive = true }) is JsonObject attributes 
				? new EntityAttributeCollection(attributes, options)
				: throw new JsonException("Cannot read an EntityAttributeCollection");
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, EntityAttributeCollection value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}