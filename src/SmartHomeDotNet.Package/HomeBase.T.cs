using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;

namespace SmartHomeDotNet
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DeviceAttribute : Attribute
	{
		public DeviceAttribute(string id)
		{
			Id = id;
		}

		/// <summary>
		/// The id of the device
		/// </summary>
		public string Id { get; }
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class AutoInitAttribute : Attribute
	{
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// A base class for an home that
	/// </summary>
	/// <remarks>
	/// This class is only an helper which eases declaration of your home.
	/// The <see cref="Current"/> will be accessible and will contains the home that is <see cref="Initializing"/>,
	/// so it can be discovered by <see cref="Automation{THome}"/> and <see cref="Scene{THome}"/> when they are created.
	/// This ensure that each component of your home are strongly attached to an instance of your <typeparamref name="THome"/>,
	/// while providing you an easy access through protected 'Home' properties.
	/// </remarks>
	/// <typeparam name="THome">The type of your home</typeparam>
	public abstract class HomeBase<THome> : HomeBase
		where THome : HomeBase<THome>
	{
		#region Discovery
		[ThreadStatic]
		private static THome _current;

		/// <summary>
		/// Gets the home **currently initializing**
		/// </summary>
		public static THome Current => _current;

		/// <inheritdoc />
		protected override IDisposable Initializing()
		{
			var previous = Interlocked.Exchange(ref _current, (THome)this);
			return Disposable.Create(() => Interlocked.CompareExchange(ref _current, previous, (THome)this));
		}
		#endregion

		protected virtual HomeDevicesManager DefaultDeviceManager { get; }
		internal HomeDevicesManager GetDefaultDeviceManager(string deviceName)
			=> DefaultDeviceManager ?? throw new NullReferenceException(
				$"No default device manager defined on '{GetType().Name}', you must specify the device manager to use for device '{deviceName}'"
				+ "(i.e. either resolve your device on device manager directly on your home, or override the DefaultDeviceManager)");

		protected virtual ISceneHost DefaultSceneHost { get; }

		internal ISceneHost GetDefaultSceneHost(string sceneName) 
			=> DefaultSceneHost ?? throw new NullReferenceException(
				$"No default scene host defined on '{GetType().Name}', you must specify the scene host to use for scene '{sceneName}'"
				+ "(i.e. either provide a scene host in you scene constructor, or override the DefaultSceneHost)");

		protected virtual IAutomationHost DefaultAutomationHost { get; }

		internal IAutomationHost GetDefaultAutomationHost(string automationName)
			=> DefaultAutomationHost ?? throw new NullReferenceException(
				$"No default automation host defined on '{GetType().Name}', you must specify the automation host to use for automation '{automationName}'"
				+ "(i.e. either provide a automation host in you automation constructor, or override the DefaultAutomationHost)");

		/// <inheritdoc />
		protected override void CreateRooms()
		{
			var rooms = FindAndCreateInstances(typeof(Room<THome>));

			TryAssignProperties(rooms);
		}

		/// <inheritdoc />
		protected override IEnumerable<IDisposable> CreateDevices()
		{
			foreach (var prop in GetType().GetProperties())
			{
				if (IsIgnored(prop) || IsIgnored(prop.PropertyType))
				{
					continue;
				}

				var attribute = prop.CustomAttributes.SingleOrDefault(attr => typeof(DeviceAttribute).IsAssignableFrom(attr.AttributeType));
				if (attribute == null)
				{
					continue;
				}

				if (prop.GetValue(this) != null)
				{
					continue;
				}
				
				var id = attribute.ConstructorArguments.First().Value as string;
				var device = GetDefaultDeviceManager(prop.Name).GetDevice(id);

				prop.SetValue(this, device);

				if (device is IDisposable d)
				{
					yield return d;
				}
			}
		}

		/// <inheritdoc />
		protected override IEnumerable<IDisposable> CreateScenes()
		{
			var scenes = FindAndCreateInstances(typeof(Scene<THome>));

			TryAssignProperties(scenes);

			return scenes.Cast<IDisposable>();
		}

		/// <inheritdoc />
		protected override IEnumerable<IDisposable> CreateAutomations()
		{
			var scenes = FindAndCreateInstances(typeof(Automation<THome>));

			TryAssignProperties(scenes);

			return scenes.Cast<IDisposable>();
		}

		private ICollection<object> FindAndCreateInstances(Type target)
		{
			return this
				.GetType()
				.Assembly
				.GetTypes()
				.Where(type => !IsIgnored(type)
					&& target.IsAssignableFrom(type) 
					&& type.GetConstructors().Any(ctor => !ctor.GetParameters().Any())
				)
				.Select(TryCreateInstance)
				.Where(inst => inst != null)
				.ToList(); // Make sure to materialize the collection only once!

			object TryCreateInstance(Type type)
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch (Exception)
				{
					this.Log().Error("Failed to create an instance of " + type.Name);

					return null;
				}
			}
		}

		private void TryAssignProperties(IEnumerable<object> instances)
		{
			var assignableProperties = this
				.GetType()
				.GetProperties()
				.Where(prop => !IsIgnored(prop))
				.Join(instances, prop => prop.PropertyType, inst => inst.GetType(), (property, value) => (property, value))
				.GroupBy(t => t.property);

			foreach (var assignableProperty in assignableProperties)
			{
				var prop = assignableProperty.Key;
				if (prop.GetValue(this) != null)
				{
					this.Log().Info($"Don't assign the property {prop.Name} as it not null.");
					continue;
				}

				if (assignableProperty.Count() > 1)
				{
					this.Log().Error($"Cannot assign property {prop.Name} as we found {assignableProperty.Count()} matching instances.");
					continue;
				}

				prop.SetValue(this, assignableProperty.First());
			}
		}

		private bool IsIgnored(PropertyInfo prop)
		{
			var initAttr = prop.CustomAttributes.SingleOrDefault(attr => typeof(AutoInitAttribute).IsAssignableFrom(attr.AttributeType));
			var isIgnored = initAttr?.NamedArguments?.SingleOrDefault(arg => arg.MemberName == nameof(AutoInitAttribute.Ignore)).TypedValue.Value;

			return isIgnored is bool b && b;
		}

		private bool IsIgnored(Type type)
		{
			var initAttr = type.CustomAttributes.SingleOrDefault(attr => typeof(AutoInitAttribute).IsAssignableFrom(attr.AttributeType));
			var isIgnored = initAttr?.NamedArguments?.SingleOrDefault(arg => arg.MemberName == nameof(AutoInitAttribute.Ignore)).TypedValue.Value;

			return isIgnored is bool b && b;
		}
	}
}