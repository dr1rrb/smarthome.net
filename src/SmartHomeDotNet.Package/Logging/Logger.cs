using System;
using System.Linq;

namespace SmartHomeDotNet.Logging
{
	public static class Logger
	{
		public delegate ILogger Provider(object target);

		private static readonly Provider _default = _ => ConsoleLogger.Instance;
		private static Provider _provider = _default;

		public static void SetProvider(Provider provider) 
			=> _provider = provider ?? _default;

		public static ILogger Log(this object owner) 
			=> _provider(owner);
	}
}