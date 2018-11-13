using System;
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
		private int _isInit;

		/// <summary>
		/// The Id of this device
		/// </summary>
		public string Id { get; private set; }

		/// <summary>
		/// The source value of the device
		/// </summary>
		protected dynamic Value { get; private set; }

		/// <inheritdoc />
		void IDeviceAdapter.Init(string id, ExpandoObject values)
		{
			if (Interlocked.CompareExchange(ref _isInit, 1, 0) == 0)
			{
				Id = id;
				Value = values;

				OnInit(values);
			}
			else
			{
				throw new InvalidOperationException("A device is an immutable object that can be init only once.");
			}
		}

		/// <summary>
		/// Callback invoked when this device get initialized
		/// </summary>
		/// <param name="value">The value of this device</param>
		protected virtual void OnInit(ExpandoObject value) { }
	}
}