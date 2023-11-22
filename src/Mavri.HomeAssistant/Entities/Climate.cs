using System;
using System.Collections.Immutable;
using System.Linq;
using Mavri.Commands;
using Mavri.Ha.Commands;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

/// <summary>
/// 
/// </summary>
/// <param name="State"></param>
/// <param name="Modes"></param>
/// <param name="Temperature">The configured temperature</param>
/// <param name="CurrentTemperature">The effective current temperature</param>
/// <param name="MinTemperature"></param>
/// <param name="MaxTemperature"></param>
public record struct Climate(
	ClimateState State, 
	ImmutableArray<ClimateState> Modes, 
	double Temperature,
	double CurrentTemperature,
	double MinTemperature,
	double MaxTemperature);

public enum ClimateState
{
	/// <summary>
	/// All activity disabled / Device is off/standby
	/// </summary>
	Off,

	/// <summary>
	/// Heating
	/// </summary>
	Heat,

	/// <summary>
	/// Cooling
	/// </summary>
	Cool,

	/// <summary>
	/// The device supports heating/cooling to a range
	/// </summary>
	HeatCool,

	/// <summary>
	/// The temperature is set based on a schedule, learned behavior, AI or some
	/// other related mechanism. User is not able to adjust the temperature
	/// </summary>
	Auto,

	/// <summary>
	/// Device is in Dry/Humidity mode
	/// </summary>
	Dry,

	/// <summary>
	/// Only the fan is on, not fan and another mode like cool
	/// </summary>
	FanOnly,
}

public sealed record ClimateEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Climate>(Id, Hub), ISupport<TurnOn>, ISupport<TurnOff>
{
	/// <inheritdoc />
	IActuator IActuatable.Actuator => Hub;

	/// <inheritdoc />
	protected override Climate Parse(EntityState raw)
	{
		var state = raw.GetState<ClimateState>(Id);
		var modes = raw.Attributes.GetArray<string>("hvac_modes", Id);
		var temperature = raw.Attributes.Get<double>("temperature", Id);
		var currentTemperature = raw.Attributes.Get<double>("current_temperature", Id);
		var min = raw.Attributes.Get<double>("min_temp", Id);
		var max = raw.Attributes.Get<double>("max_temp", Id);

		return new Climate(
			state,
			modes.Select(m => Enum.Parse<ClimateState>(m.Replace("_", ""), ignoreCase: true)).ToImmutableArray(),
			//presets,
			temperature,
			currentTemperature,
			min,
			max);
	}
}