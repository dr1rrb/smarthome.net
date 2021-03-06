﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SmartHomeDotNet.Hass.Api
{
	/// <summary>
	/// A dynamic service data object that can be used to easily convert from strongly typed object model,
	/// to the request format.
	/// </summary>
	public class ServiceData : DynamicObject
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

		/// <inheritdoc />
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _values.Keys;
		}

		/// <inheritdoc />
		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (!(indexes.FirstOrDefault() is string propertyName))
			{
				result = default;
				return false;
			}

			if (!_values.TryGetValue(propertyName, out result))
			{
				result = _values[propertyName] = new ServiceData();
			}

			return true;
		}

		/// <inheritdoc />
		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			if (!(indexes.FirstOrDefault() is string propertyName))
			{
				return false;
			}

			_values[propertyName] = value;
			return true;
		}

		/// <inheritdoc />
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!_values.TryGetValue(binder.Name, out result))
			{
				result = _values[binder.Name] = new ServiceData();
			}

			return true;
		}

		/// <inheritdoc />
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_values[binder.Name] = value;
			return true;
		}
	}
}