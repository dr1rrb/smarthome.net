using System;
using System.Linq;

namespace SmartHomeDotNet.SmartHome.Automations
{
	/// <summary>
	/// An automation for a <typeparamref name="THome"/>
	/// </summary>
	/// <typeparam name="THome">Type of the home where this automation takes place</typeparam>
	public abstract class Automation<THome> : Automation
		where THome : HomeBase<THome>
	{
		/// <summary>
		/// Gets the hosing home
		/// </summary>
		protected THome Home { get; } = HomeBase<THome>.Current;

		/// <inheritdoc />
		protected Automation(string name, IAutomationHost host = null)
			: base(name, host ?? HomeBase<THome>.Current.GetDefaultAutomationHost(name))
		{
		}

		/// <inheritdoc />
		protected Automation(string id, string name, IAutomationHost host = null)
			: base(id, name, host ?? HomeBase<THome>.Current.GetDefaultAutomationHost(name))
		{
		}
	}
}