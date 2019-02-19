using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// A scene for a <typeparamref name="THome"/>
	/// </summary>
	/// <typeparam name="THome">Type of the home where this scene takes place</typeparam>
	public abstract class Scene<THome> : Scene
		where THome : HomeBase<THome>
	{
		/// <summary>
		/// Gets the hosing home
		/// </summary>
		protected THome Home { get; } = HomeBase<THome>.Current;

		/// <inheritdoc />
		protected Scene(string name, ISceneHost host = null)
			: base(name, host ?? HomeBase<THome>.Current.GetDefaultSceneHost(name))
		{
		}

		/// <inheritdoc />
		protected Scene(string id, string name, ISceneHost host = null)
			: base(id, name, host ?? HomeBase<THome>.Current.GetDefaultSceneHost(name))
		{
		}
	}
}