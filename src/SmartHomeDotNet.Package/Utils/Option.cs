using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHomeDotNet.Utils
{
	internal struct Option<T>
	{
		private Option(T value)
		{
			_hasValue = true;
			_value = value;
		}

		private readonly bool _hasValue;
		private readonly T _value;

		public bool MatchNone() => !_hasValue;

		public bool MatchSome() => _hasValue;

		public bool MatchSome(out T value)
		{
			value = _value;
			return _hasValue;
		}

		public static implicit operator Option<T>(T value) => new Option<T>(value);
		public static implicit operator T(Option<T> option) => option._value;
	}
}
