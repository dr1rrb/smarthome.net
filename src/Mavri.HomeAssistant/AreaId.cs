using System;
using System.Linq;

namespace Mavri.Ha;

public readonly struct AreaId
{
	private readonly string _value;

	private AreaId(string value)
	{
		_value = value;
	}

	/// <inheritdoc />
	public override string ToString()
		=> _value;

	public static implicit operator AreaId(string value)
		=> new(value);

	public static explicit operator string(AreaId id)
		=> id._value;
}