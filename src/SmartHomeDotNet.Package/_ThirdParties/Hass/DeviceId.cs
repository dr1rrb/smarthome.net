using System;
using System.Linq;

namespace SmartHomeDotNet.Hass;

public readonly struct DeviceId
{
	private readonly string _value;

	private DeviceId(string value)
	{
		_value = value;
	}

	/// <inheritdoc />
	public override string ToString()
		=> _value;

	public static implicit operator DeviceId(string value)
		=> new(value);

	public static explicit operator string(DeviceId id)
		=> id._value;
}