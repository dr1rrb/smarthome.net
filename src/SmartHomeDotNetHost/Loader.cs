using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet
{
	public class Loader
	{
		private readonly string _directory;

		public Loader(string directory)
		{
			_directory = directory;
		}

		public void Run()
		{
			var activables = Directory
				.GetFiles(_directory, "*.dll", SearchOption.TopDirectoryOnly)
				.Select(TryLoad)
				.Where(a => a != null)
				.SelectMany(a => a.GetTypes())
				.Where(t => typeof(IActivable).IsAssignableFrom(t))
				.Select(TryCreate)
				.Where(a => a != null);

			foreach (var activable in activables)
			{
				try
				{
					activable.Enable();
				}
				catch (Exception e)
				{
					this.Log().Error($"Failed to enable {activable.GetType()}", e);
				}
			}

			Assembly TryLoad(string dll)
			{
				try
				{
					return Assembly.LoadFile(dll);
				}
				catch (Exception e)
				{
					this.Log().Error($"Failed to load {dll}", e);

					return null;
				}
			}

			IActivable TryCreate(Type activable)
			{
				try
				{
					return (IActivable) Activator.CreateInstance(activable);
				}
				catch (Exception e)
				{
					this.Log().Error($"Failed to instanciate {activable}", e);

					return null;
				}
			}
		}
	}
}
