using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HomeGenerator.Cli.Utils;
using Newtonsoft.Json.Linq;
using SmartHomeDotNet.Hass;
using SmartHomeDotNet.Hass.Api;
using SmartHomeDotNet.SmartHome.Commands;
using SmartHomeDotNet.SmartHome.Devices_2;
using SmartHomeDotNet.Utils;

namespace HomeGenerator.Cli.Generation;

public class Generator : ICodeGenTool
{
	private static readonly string[] _staticReplacements = { "Du", "De", "DeLa" };
	private static readonly JsonSerializerOptions JsonReadOpts;

	string ICodeGenTool.Version =>"1";

	static Generator()
	{
		JsonReadOpts = HomeAssistantWebSocketApi.CreateDefaultJsonReadOptions();
		JsonReadOpts.Converters.Add(SourceJson<EntityInfo>.Instance);
		JsonReadOpts.Converters.Add(SourceJson<DeviceInfo>.Instance);
		JsonReadOpts.Converters.Add(SourceJson<AreaInfo>.Instance);
		JsonReadOpts.Converters.Add(SourceJson<EntityState>.Instance);
	}

	private readonly string _homeAssistantHost;
	private readonly string _authToken;

	public Generator(string homeAssistantHost, string authToken)
	{
		_homeAssistantHost = homeAssistantHost;
		_authToken = authToken;
	}

	public async ValueTask<string> Generate(string ns, CancellationToken ct)
	{
		using var client = new HomeAssistantWebSocketApi(_homeAssistantHost, _authToken);

		//var result = await client.Send<dynamic>(new GetConfigCommand(), ct);

		//string result;

		//result = await client.Send(new GetConfigCommand(), ct);

		//await File.WriteAllTextAsync("config.json", result, ct);

		//result = await client.Send(new GenericCommand("config/device_registry/list"), ct);

		//await File.WriteAllTextAsync("device_registry.json", result, ct);

		//result = await client.Send(new GenericCommand("config/entity_registry/list"), ct);

		//await File.WriteAllTextAsync("entity_registry.json", result, ct);

		//

		//await File.WriteAllTextAsync("states.json", result, ct);

		//var areas = await client.Send<ImmutableArray<AreaInfo>>(new GenericCommand("config/area_registry/list"), ct);

		var config = await client.Send<Config>(new GetConfigCommand(), ct);
		var (entities, enums) = await GetEntities(client, ct);
		var devices = await GetDevices(client, entities, ct);
		var areas = await GetAreas(client, devices, ct);

		//var states = await client.Send<ImmutableArray<EntityState>>(new GenericCommand("get_states"), ct);

		Directory.CreateDirectory($"..\\..\\..\\G\\entities");
		Directory.CreateDirectory($"..\\..\\..\\G\\devs");
		Directory.CreateDirectory($"..\\..\\..\\G\\areas");

		foreach (var @enum in GenerateEnums(ns, enums))
		{
			await File.WriteAllTextAsync($"..\\..\\..\\G\\entities\\{@enum.name}.g.cs", @enum.code, ct);
		}

		foreach (var entityRegistry in GenerateEntityRegistry(ns, entities))
		{
			await File.WriteAllTextAsync($"..\\..\\..\\G\\entities\\{entityRegistry.name}.cs", entityRegistry.code, ct);
		}

		foreach (var dev in GenerateDevices(ns, devices))
		{
			await File.WriteAllTextAsync($"..\\..\\..\\G\\devs\\{dev.name}.g.cs", dev.code, ct);
		}

		foreach (var area in GenerateAreas(ns, config.Name, areas))
		{
			await File.WriteAllTextAsync($"..\\..\\..\\G\\areas\\{area.name}.g.cs", area.code, ct);
		}

		return "";
	}

	public static class NS
	{
		// Where we have the entities (ButtonEntity, LightEntity)
		public const string Entities = "global::HomeGenerator.Cli";
	}

	public static class T
	{
		public static readonly string HA = "global::" + typeof(IHomeAssistantHub).FullName;
		public static readonly string Actuator = "global::" + typeof(IDeviceActuator).FullName;
		public static readonly string AsyncOp = "global::" + typeof(AsyncContextOperation).FullName;
		public static readonly string ICommand = "global::" + typeof(ICommand).FullName;

		public static readonly string EntityId = "global::" + typeof(EntityId).FullName;
		public static readonly string IEntity = "global::" + typeof(IEntity).FullName;
		public static readonly string IEntityRegistry = "global::" + typeof(IEntityRegistry).FullName;
		public static readonly string Entity = "global::HomeGenerator.Cli.Entity";

		public static readonly string DeviceId = "global::" + typeof(DeviceId).FullName;
		public static readonly string IDevice = "global::" + typeof(IDevice).FullName;
		public static readonly string IDeviceRegistry = "global::" + typeof(IDeviceRegistry).FullName;
		public static readonly string Device = "global::HomeGenerator.Cli.Device";


		public static readonly string EntitiesManager = "global::" + typeof(EntitiesManager).FullName;
		public static readonly string SocketApi = "global::" + typeof(HomeAssistantWebSocketApi).FullName;
		public static readonly string RestApi = "global::" + typeof(HomeAssistantHttpApi).FullName;
	}

	/// <param name="Type">Type of the enum</param>
	/// <param name="Values">Escaped values of the enum</param>
	private record EntityEnumGen(string Type, ImmutableArray<string> Values);

	/// <param name="Info"></param>
	/// <param name="State">A state example captured at generation for debug purposes</param>
	/// <param name="Type">The type of the entity (e.g. LightEntity)</param>
	/// <param name="Property">The name of the entity in the registry. Note: We expect to **NOT** have the same name for entities while in a device.</param>
	private record EntityGen(EntityInfo Info, EntityState? State, string Type, string Property)
	{
		public EntityId Id => Info.Id;

		/// <summary>
		/// The type (and name) of the component registry that holds the entity.
		/// </summary>
		public string Component { get; } = GetComponentName(Info.Id.Component);

		public static string GetComponentName(Component component)
			=> ToCSharpCamel(component.Name);

		public string Summary => $"{Info.Name ?? Info.OriginalName} ({Id})";

		//public static string GetDeviceProperty(EntityInfo entity)
		//	=> ToCsharpName("Entity", entity switch
		//	{
		//		{ HasName: true, Name: { Length: > 0 } name } => name,
		//		{ OriginalName: { Length: > 0 } name } => name,
		//		_ => entity.Id.Id.ToString()
		//	});

		//public static string GetEntityPropertyNameLight(EntityInfo entity)
		//	=> ToCsharpName("Entity", entity.Id.Id);

		//public EntityGen WithSafePropertyName()
		//	=> this with { Property = GetEntityPropertyNameLight(Info) };
	}

	private record DeviceGen(DeviceInfo Info, ImmutableArray<EntityGen> Entities)
	{
		/// <summary>
		/// The type of the device (e.g. ChristmasString001)
		/// </summary>
		public string Type { get; init; } = GetTypeName(Info);

		/// <summary>
		/// The name of the device in the registry
		/// </summary>
		public string Property { get; init; } = GetPropertyName(Info);

		public string Summary => $"{Info.FriendlyName ?? Info.Name} ({Info.Model} by {Info.Manufacturer})";

		static string GetTypeName(DeviceInfo device)
			=> ToCsharpName("Device", device switch
			{
				{ Name: { Length: > 0 } name } => name,
				_ => device.Id.ToString()
			});

		static string GetPropertyName(DeviceInfo device)
			=> ToCsharpName("_", device switch
			{
				{ FriendlyName: { Length: > 0 } name } => name,
				{ Name: { Length: > 0 } name } => name,
				_ => device.Id.ToString()
			});

		public IEnumerable<string> Tokens()
		{
			yield return Property;

			yield return Type;

			yield return GetTypeName(Info); // The default type name might have been overridden

			if (Info is { FriendlyName: { Length: > 0 } friendlyName })
			{
				yield return friendlyName;
			}

			if (Info is { Name: { Length: > 0 } name })
			{
				yield return name;
			}

			yield return Info.Id;
		}
	}

	private record AreaGen(AreaInfo Info, ImmutableArray<DeviceGen> Devices)
	{
		public string Name => ToCSharpCamel(Info.Name ?? Info.Id);

		public string Summary => $"{Info.Name} ({Info.Id})";

		public IEnumerable<string> Tokens()
		{
			yield return Name;

			yield return Info.Id;
		}
	}

	private IEnumerable<(string name, string code)> GenerateEnums(string ns, ImmutableArray<EntityEnumGen> enums)
	{
		foreach (var @enum in enums)
		{
			yield return (@enum.Type, $$"""
					{{this.GetFileHeader()}}
					using SmartHomeDotNet.Hass;

					namespace {{ns}};

					{{this.GetCodeGenAttribute()}}
					public enum {{@enum.Type}}
					{
						{{@enum.Values.Align(1, ", ")}}
					}
					""");
		}
	}

	private IEnumerable<(string name, string code)> GenerateEntityRegistry(string ns, ImmutableArray<EntityGen> entities)
	{
		var entitiesByComponents = entities.GroupBy(entity => entity.Id.Component).ToList();

		yield return ("_EntityRegistry", $$"""
				{{this.GetFileHeader()}}
				using SmartHomeDotNet.Hass;

				namespace {{ns}};

				{{this.GetCodeGenAttribute()}}
				public partial record EntityRegistry({{T.HA}} Hub) : {{T.IEntityRegistry}}
				{
					/// <inheritdoc />
					public {{T.IEntity}}? Get({{T.EntityId}} id)
						=> id.Component.Name switch
						{
							{{entitiesByComponents.Select(component => $"\"{component.Key.Name}\" => {EntityGen.GetComponentName(component.Key)}.Get(id),").Align(3)}}
							_ => default
						};
				}
				""");

		foreach (var component in entitiesByComponents)
		{
			var componentName = EntityGen.GetComponentName(component.Key);
			yield return ($"_EntityRegistry.{componentName}", $$"""
					{{this.GetFileHeader()}}
					using SmartHomeDotNet.Hass;

					namespace {{ns}};

					public partial record EntityRegistry
					{
						public {{componentName}}Registry {{componentName}} { get; } = new(Hub);

						{{this.GetCodeGenAttribute()}}
						public partial record {{componentName}}Registry({{T.HA}} Hub) : {{T.IEntityRegistry}}
						{
							/// <inheritdoc />
							public {{T.IEntity}}? Get({{T.EntityId}} id)
								=> id.Id switch
								{
									{{component.Select(entity => $"\"{entity.Id.Id}\" => {entity.Property},").Align(4)}}
									_ => default
								};

							{{component.Select(entity => $$""""
							/// <summary>
							/// {{entity.Summary}}
							/// </summary>
							/// <remarks>
							/// Source JSON:
							/// {{SourceJson.ToString(entity.Info)?.Align(0, "///")}}
							/// </remarks>
							{{(entity.State is { } state && SourceJson.ToString(state) is { } stateJson
								? $$"""
								/// <remarks>
								/// Example of state:
								/// {{stateJson.Align(0, "///")}}
								/// </remarks>
								"""
								: "///<remarks>Home assistant didn't provided any state for this entity at generation time.</remarks>")}}
							public {{entity.Type}} {{entity.Property}} { get; } = new(new {{T.EntityId}}("{{entity.Id.Component}}", "{{entity.Id.Id}}"), Hub);

							"""").Align(2)}}
						}
					}
					""");
		}

	}

	private IEnumerable<(string name, string code)> GenerateDevices(string ns, ImmutableArray<DeviceGen> devices)
	{
		yield return ("_DeviceRegistry", $$"""
				{{this.GetFileHeader()}}
				using SmartHomeDotNet.Hass;

				namespace {{ns}};

				{{this.GetCodeGenAttribute()}}
				public partial record DeviceRegistry : {{T.IDeviceRegistry}}
				{
					public DeviceRegistry(EntityRegistry entities)
					{
						{{devices.Select(dev => $"{dev.Property} = new {dev.Type}(entities);").Align(2)}}
					}

					/// <inheritdoc />
					public {{T.IDevice}}? Get({{T.DeviceId}} id)
						=> ((string)id) switch
						{
							{{devices.Select(dev => $"\"{dev.Info.Id}\" => {dev.Property},").Align(3)}}
							_ => default
						};

					{{devices.Select(dev => $$"""
						/// <summary>
						/// {{dev.Summary}}
						/// </summary>
						public {{dev.Type}} {{dev.Property}} { get; }
						""").Align(1)}}
				}
				""");

		foreach (var dev in devices)
		{
			var entities = dev.Entities
				.Select(entity => (entity, name: GetDeviceProperty(entity.Info)))
				.ToImmutableArray()
				.DeDuplicate(entity => entity.name, StringComparer.Ordinal, (e, i) => (e.entity, $"{e.name}_{i}"));

			yield return (dev.Type, $$"""
				{{this.GetFileHeader()}}
				using SmartHomeDotNet.Hass;
				
				namespace {{ns}};

				/// <summary>
				/// {{dev.Summary}})
				/// </summary>
				/// <remarks>
				/// Source JSON:
				/// {{SourceJson.ToString(dev.Info)?.Align(0, "///")}}
				/// </remarks>
				{{this.GetCodeGenAttribute()}}
				public sealed partial record {{dev.Type}} : {{T.Device}}
				{
					private readonly EntityRegistry _entities;

					/// <summary>
					/// Creates an instance of device {{dev.Info.Id}}.
					/// </summary>
					public {{dev.Type}}(EntityRegistry entities) : base("{{dev.Info.Id}}", entities.Hub)
					{
						_entities = entities;
					}
				
					{{entities.Select(e => $$"""
						/// <summary>
						/// {{e.entity.Summary}}
						/// </summary>
						public {{e.entity.Type}} {{e.name}} => _entities.{{e.entity.Component}}.{{e.entity.Property}};
						""").Align(1)}}
				}
				""");

			string GetDeviceProperty(EntityInfo entity)
			{
				var name = entity switch
				{
					{ HasName: true, Name: { Length: > 0 } n } => n,
					{ OriginalName: { Length: > 0 } n } => n,
					_ => entity.Id.Id.ToString()
				};

				foreach (var value in dev.Tokens())
				{
					name = name.Replace(value, "", StringComparison.OrdinalIgnoreCase);
				}

				name = ToCSharpCamel(name.Trim());

				foreach (var value in dev.Tokens())
				{
					name = name.Replace(value, "", StringComparison.OrdinalIgnoreCase);
				}

				return ToCsharpName("_", name.Trim() is { Length: > 0 } finalName ? finalName : entity.Id.Component);
			}
		}
	}

	private IEnumerable<(string name, string code)> GenerateAreas(string ns, string? homeName, ImmutableArray<AreaGen> areas)
	{
		homeName ??= "AreaRegistry";

		yield return ("_" + homeName, $$"""
				{{this.GetFileHeader()}}
				namespace {{ns}};

				{{this.GetCodeGenAttribute()}}
				public partial record {{homeName}} : {{T.HA}}, IDisposable
				{
					/// <inheritdoc />
					public Uri BaseUri { get; } = new Uri("https://{{_homeAssistantHost}}");

					public {{T.SocketApi}} SocketApi { get; } = new("{{_homeAssistantHost}}", "{{_authToken}}");

					public {{T.RestApi}} RestApi { get; } = new("{{_homeAssistantHost}}", "{{_authToken}}");

					/// <summary>
					/// Registry of entities of {{homeName}}.
					/// </summary>
					public EntityRegistry Entities { get; }
					/// <inheritdoc />
					{{T.IEntityRegistry}} {{T.HA}}.Entities => Entities;
				
					/// <summary>
					/// Registry of devices of {{homeName}}.
					/// </summary>
					public DeviceRegistry Devices { get; }
					/// <inheritdoc />
					{{T.IDeviceRegistry}} {{T.HA}}.Devices => Devices;

					private {{T.EntitiesManager}} _manager;

					/// <summary>
					/// Creates a new instance of {{homeName}}.
					/// </summary>
					public {{homeName}}()
					{
						Entities = new EntityRegistry(this);
						Devices = new DeviceRegistry(Entities);
					
						{{areas.Select(area => $"{area.Name} = new(Devices);").Align(2)}}

						_manager = new(Entities, SocketApi);
					}

					{{areas.Select(area => $$"""
							/// <summary>
							/// {{area.Summary}}
							/// </summary>
							public {{area.Name}} {{area.Name}} { get; }
							""").Align(1)}}

					/// <inheritdoc />
					{{T.AsyncOp}} {{T.Actuator}}.Execute({{T.ICommand}} command, params object[] devices)
						=> throw new global::System.NotImplementedException();

					/// <inheritdoc />
					{{T.AsyncOp}} {{T.Actuator}}<{{T.EntityId}}>.Execute({{T.ICommand}} command, params {{T.EntityId}}[] devices)
						=> throw new global::System.NotImplementedException();

					/// <inheritdoc />
					{{T.AsyncOp}} {{T.Actuator}}<{{T.DeviceId}}>.Execute({{T.ICommand}} command, params {{T.DeviceId}}[] devices)
						=> throw new global::System.NotImplementedException();

					/// <inheritdoc />
					public void Dispose()
					{
						_manager.Dispose();
						SocketApi.Dispose();
						RestApi.Dispose();
					}
				}
				"""
		);

		foreach (var area in areas)
		{
			yield return (area.Name, $$"""
					{{this.GetFileHeader()}}
					namespace {{ns}};

					/// <summary>
					/// {{area.Summary}}
					/// </summary>
					/// <remarks>
					/// Source JSON:
					/// {{SourceJson.ToString(area.Info)?.Align(0, "///")}}
					/// </remarks>
					public partial record {{area.Name}}
					{
						private readonly DeviceRegistry _devices;

						internal {{area.Name}}(DeviceRegistry devices)
						{
							_devices = devices;
						}

						{{area.Devices.Select(dev => $$"""
							/// <summary>
							/// {{dev.Summary}})
							/// </summary>
							public {{dev.Type}} {{GetDeviceProperty(dev)}} => _devices.{{dev.Property}};
							""").Align(1)}}
					}
					""");

			string GetDeviceProperty(DeviceGen device)
			{
				var name = device.Property;

				foreach (var token in area.Tokens())
				{
					name = name.Replace(token, "", StringComparison.OrdinalIgnoreCase);
				}

				foreach (var token in _staticReplacements)
				{
					name = name.TrimEnd(token, StringComparison.Ordinal);
				}

				return name;
			}
		}
	}

	private async Task<(ImmutableArray<EntityGen> entities, ImmutableArray<EntityEnumGen> enums)> GetEntities(HomeAssistantWebSocketApi client, CancellationToken ct)
	{
		var entities = await client.Send<ImmutableArray<EntityInfo>>(new GenericCommand("config/entity_registry/list"), JsonReadOpts, ct);
		var states = await client.Send<ImmutableArray<EntityState>>(new GetStatesCommand(), JsonReadOpts, ct);

		var currentStates = states.ToDictionary(state => state.EntityId);
		//var previousGen = Array.Empty<EntityGen>().ToDictionary(entity => entity.Id);
		var manualConfigs = typeof(EntityAttribute).Assembly.GetCustomAttributes<EntityAttribute>().ToDictionary(entity => entity.EntityId);
		var enums = new List<EntityEnumGen>();
		//var entitiesToGen = entities.Select(entity => new EntityGen(entity)).ToList();

		//// De-duplicate entity property names
		//foreach (var duplicates in entitiesToGen.GroupBy(entity => entity.Component + entity.Property, StringComparer.Ordinal).Where(devGroup => devGroup.Count() > 1))
		//{
		//	foreach (var entity in duplicates)
		//	{
		//		entitiesToGen.Remove(entity);
		//		entitiesToGen.Add(entity with { Property = ToCSharpCamel(entity.Id.Id, canIgnoreSomeChars: false) });
		//	}
		//}

		//return entitiesToGen.ToImmutableArray();

		var entitiesToGen = entities
			.Distinct(EntityIdComparer.Instance)
			.Select(entity =>
			{
				var state = currentStates.GetValueOrDefault(entity.Id);
				var type = GetEntityType(entity, state);
				var property = GetEntityPropertyNameLight(entity);

				return new EntityGen(entity, state, type, property);
			})
			.ToImmutableArray()
			.DeDuplicate(
				entity => entity.Component + entity.Property,
				StringComparer.Ordinal,
				entity => entity with { Property = ToCSharpCamel(entity.Id.Id, canIgnoreSomeChars: false) });



		return (entitiesToGen, enums.ToImmutableArray());

		static string GetEntityPropertyNameLight(EntityInfo entity)
			=> ToCsharpName("Entity", entity.Id.Id);

		string GetEntityType(EntityInfo entityInfo, EntityState? currentState)
		{
			// TODO: Use reflection / roslyn to find attributes
			if (manualConfigs.TryGetValue(entityInfo.Id, out var manualConfig))
			{
				return manualConfig.EntityType.FullName!;
			}
			else if (GetEntityTypeForComponent(entityInfo) is { } componentType)
			{
				return componentType;
			}
			else if (currentState is not null && GetEntityTypeFromCurrentState(entityInfo, currentState) is { } stateType)
			{
				// Note: Here we could also fetch history to validate types
				return stateType;
			}
			else
			{
				return $"{NS.Entities}.{nameof(UnknownEntity)}";
			}
		}

		string? GetEntityTypeFromCurrentState(EntityInfo info, EntityState currentState)
			=> info.Id.Component.Name switch
			{
				"sensor" when (currentState?.Attributes.TryGet<string>("unit_of_measurement", out var unit) ?? false) && unit is { Length: > 0 } => $"{NS.Entities}.{nameof(DoubleEntity)}",
				"sensor" when currentState?.Attributes.IsSet("state_class") ?? false => $"{NS.Entities}.{nameof(DoubleEntity)}",
				"sensor" when currentState?.Attributes.TryGet<string>("device_class", out var deviceClass) ?? false => deviceClass switch
				{
					"enum" => $"{NS.Entities}.EnumEntity<{CreateEnum(info, currentState, "options")}>",
					"timestamp" => $"{NS.Entities}.{nameof(TimestampEntity)}",
					_ => $"{NS.Entities}.{nameof(SensorEntity)}",
				},
				"sensor" => $"{NS.Entities}.{nameof(SensorEntity)}",
				"event" => $"{NS.Entities}.EventEntity<{CreateEnum(info, currentState, "event_types")}>",
				"select" => $"{NS.Entities}.SelectEntity<{CreateEnum(info, currentState, "options")}>",
				"input_select" => $"{NS.Entities}.InputSelectEntity<{CreateEnum(info, currentState, "options")}>",
				_ => null,
			};

		static string? GetEntityTypeForComponent(EntityInfo info)
			=> info.Id.Component.Name switch
			{
				"button" => $"{NS.Entities}.{nameof(ButtonEntity)}",
				"binary_sensor" => $"{NS.Entities}.{nameof(BinarySensorEntity)}",
				"light" => $"{NS.Entities}.{nameof(LightEntity)}",
				"switch" => $"{NS.Entities}.{nameof(SwitchEntity)}",
				"camera" => $"{NS.Entities}.{nameof(CameraEntity)}",
				"person" => $"{NS.Entities}.{nameof(PersonEntity)}",
				"cover" => $"{NS.Entities}.{nameof(CoverEntity)}",
				"device_tracker" => $"{NS.Entities}.{nameof(DeviceTrackerEntity)}",
				"input_datetime" => $"{NS.Entities}.{nameof(InputDateTimeEntity)}",
				"input_text" => $"{NS.Entities}.{nameof(InputTextEntity)}",
				"zone" => $"{NS.Entities}.{nameof(ZoneEntity)}",
				"climate" => $"{NS.Entities}.{nameof(ClimateEntity)}",
				"alarm_control_panel" => $"{NS.Entities}.{nameof(AlarmControlPanelEntity)}",
				"fan" => $"{NS.Entities}.{nameof(FanEntity)}",
				"number" => $"{NS.Entities}.{nameof(NumberEntity)}",
				"input_number" => $"{NS.Entities}.{nameof(InputNumberEntity)}",
				"lock" => $"{NS.Entities}.{nameof(LockEntity)}",
				"siren" => $"{NS.Entities}.{nameof(SirenEntity)}",

				// Ignored for now
				// "automation"
				// "script"
				// "update"
				//"weather" => $"{NS.Entities}.{nameof(WeatherEntity)}",
				// "calendar"
				// "stt"
				// "tts"
				// "wake_word"
				// "remote"
				_ => null,
			};

		string CreateEnum(EntityInfo info, EntityState currentState, string valuesKey)
		{
			var type = GetEntityPropertyNameLight(info) + ToCSharpCamel(valuesKey);
			var values = currentState.Attributes.GetArray<string>(valuesKey, info.Id).Select(value => ToCsharpName("_", value)).ToImmutableArray();

			enums.Add(new EntityEnumGen(type, values));

			return type;
		}
	}

	private static async Task<ImmutableArray<DeviceGen>> GetDevices(HomeAssistantWebSocketApi client, ImmutableArray<EntityGen> entities, CancellationToken ct)
	{
		var devices = await client.Send<ImmutableArray<DeviceInfo>>(new GenericCommand("config/device_registry/list"), JsonReadOpts, ct);
		var devicesToGen = entities
			.Where(entity => entity.Info.DeviceId is { Length: > 0 })
			.GroupBy(entity => entity.Info.DeviceId!)
			.Join(devices, g => g.Key, d => d.Id, (devEntities, device) => new DeviceGen(device, devEntities.ToImmutableArray()))
			.ToImmutableArray()
			.DeDuplicate(
				dev => dev.Type,
				StringComparer.OrdinalIgnoreCase,
				dev => dev with
				{
					Type = dev.Type + '_' + dev.Info.Id,
					Property = dev.Property + '_' + dev.Info.Id,
				});

		// De-duplicate device types
		//foreach (var duplicates in devicesToGen.GroupBy(dev => dev.Type, StringComparer.OrdinalIgnoreCase).Where(devGroup => devGroup.Count() > 1))
		//{
		//	foreach (var dev in duplicates)
		//	{
		//		devicesToGen.Remove(dev);
		//		devicesToGen.Add(dev with
		//		{
		//			Type = dev.Type + '_' + dev.Device.Id,
		//			Property = dev.Property + '_' + dev.Device.Id,
		//		});
		//	}
		//}

		return devicesToGen;
	}

	private static async Task<ImmutableArray<AreaGen>> GetAreas(HomeAssistantWebSocketApi client, ImmutableArray<DeviceGen> devices, CancellationToken ct)
	{
		var areas = await client.Send<ImmutableArray<AreaInfo>>(new GenericCommand("config/area_registry/list"), JsonReadOpts, ct);
		var areasToGen = areas
			.Select(area => new AreaGen(area, devices.Where(dev => dev.Info.AreaId == area.Id).ToImmutableArray()))
			.ToImmutableArray();

		return areasToGen;
	}

	public static string ToCsharpName(string prefix, string name)
	{
		name = ToCSharpCamel(name);
		return name is null or { Length: 0 } || char.IsNumber(name[0]) ? prefix + name : name;
	}

	public static string ToCSharpCamel(string name, bool canIgnoreSomeChars = true)
	{
		if (name.Length == 0)
		{
			return name;
		}

		var result = new StringBuilder((int)(name.Length * 1.2));
		var nextIsUpper = true;
		var lastWasDigit = false;

		for (var i = 0; i < name.Length; i++)
		{
			var c = name[i];
			if (canIgnoreSomeChars && !(lastWasDigit && IsDigit(i + 1)) && c is '_' or ' ' or '-' or '\'')
			{
				nextIsUpper = true;
			}
			else if (c is '&')
			{
				nextIsUpper = true;
				lastWasDigit = false;
				result.Append("And");
			}
			else if (!char.IsLetterOrDigit(c))
			{
				nextIsUpper = true;
				lastWasDigit = false;
				result.Append('_');
			}
			else if (nextIsUpper)
			{
				nextIsUpper = false;
				lastWasDigit = char.IsDigit(c);
				result.Append(char.ToUpperInvariant(c));
			}
			else
			{
				// nextIsUpper = false; // Already false, no needs to update it
				lastWasDigit = char.IsDigit(c);
				result.Append(c);
			}
		}

		return result.ToString();

		bool IsDigit(int i)
			=> i < name.Length && char.IsDigit(name[i]);
	}
}

internal interface ICodeGenTool
{
	string Version { get; }
}

internal static class CodeGenToolExtensions
{
	public static string GetCodeGenAttribute(this ICodeGenTool tool)
		=> $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{tool.GetType().Name}"", ""{tool.Version}"")]";

	public static string GetFileHeader(this ICodeGenTool tool, int aligned = 0)
		=> $@"//----------------------
		// <auto-generated>
		//	Generated by the {tool.GetType().Name} v{tool.Version}. DO NOT EDIT!
		//	Manual changes to this file will be overwritten if the code is regenerated.
		// </auto-generated>
		//----------------------
		#pragma warning disable
		#nullable enable".Align(Math.Max(aligned - 1, 0));
}