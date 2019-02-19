using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using SmartHomeDotNet.SmartHome.Automations;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;

namespace SmartHomeDotNet
{
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
	}
}