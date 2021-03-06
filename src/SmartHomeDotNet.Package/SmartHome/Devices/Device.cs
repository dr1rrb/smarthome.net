﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Threading;

namespace SmartHomeDotNet.SmartHome.Devices
{
	/// <summary>
	/// The base class for strong typed devices implementation
	/// </summary>
	public class Device : IDeviceAdapter, IDevice
	{
		private DeviceState _state;
		private dynamic _raw;

		/// <inheritdoc />
		public object Id => GetState().DeviceId;

		/// <inheritdoc />
		public IDeviceHost Host { get; private set; }

		/// <summary>
		/// The raw source value of the device
		/// </summary>
		protected dynamic Raw => _raw ?? (_raw = GetState().ToDynamic());

		/// <inheritdoc />
		void IDeviceAdapter.Init(DeviceState state, IDeviceHost host)
		{
			state = state ?? throw new ArgumentNullException(nameof(state));
			host = host ?? throw new ArgumentNullException(nameof(host));

			if (Interlocked.CompareExchange(ref _state, state, null) == null)
			{
				Host = host;
				OnInit();
			}
			else
			{
				throw new InvalidOperationException("A device is an immutable object that can be init only once.");
			}
		}

		/// <summary>
		/// Callback invoked when this device get initialized
		/// </summary>
		protected virtual void OnInit() { }

		/// <summary>
		/// Try to get the value of a property, if value is missing returns null
		/// </summary>
		/// <param name="property">Name of the property</param>
		/// <returns>The value of the property or `null` is the property was not set.</returns>
		protected string GetValueOrDefault(string property)
			=> GetState().Properties.GetValueOrDefault(property);

		/// <summary>
		/// Try to get the value of a property
		/// </summary>
		/// <param name="property">Name of the property</param>
		/// <param name="value">The value of the property</param>
		/// <returns>A boolean which indicate if the property was set or not.</returns>
		protected bool TryGetValue(string property, out string value)
			=> GetState().Properties.TryGetValue(property, out value);

		private DeviceState GetState()
		{
			if (_state == null)
			{
				throw new InvalidOperationException("Device not initialized");
			}

			return _state;
		}
	}
}