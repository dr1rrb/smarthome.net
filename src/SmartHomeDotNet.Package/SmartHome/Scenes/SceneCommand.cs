using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Scenes
{
	/// <summary>
	/// Commands to send to a scene
	/// </summary>
	public enum SceneCommand
	{
		/// <summary>
		/// Request to start execution of a scene
		/// </summary>
		Start,

		/// <summary>
		/// Request to stop any pending execution of a scene
		/// </summary>
		Stop
	}
}