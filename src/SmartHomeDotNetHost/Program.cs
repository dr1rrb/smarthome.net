using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet
{
	public class Program
	{
		public static string WorkingDirectory =
#if DEBUG
			Path.Combine(Directory.GetCurrentDirectory(), "bin/Debug/netcoreapp2.1/smarthome");
#else
			"/smarthome";
#endif

		public static void Main(string[] args)
		{
			try
			{
				Logger.SetProvider(_ => SerilogAdapter.Instance);
				Directory.CreateDirectory(WorkingDirectory);
				BuildWebHost(args).Run();
			} 
			catch (Exception e)
			{
				Log.Fatal(e, "Host terminated unexpectedly");

				throw;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.ConfigureAppConfiguration(config => config
					.SetBasePath(WorkingDirectory)
					.AddJsonFile("config.json", optional: true))
				.UseSerilog((host, logger) => logger
					.MinimumLevel.Is(host.Configuration.GetValue<LogEventLevel>("LogLevel"))
					.WriteTo.Console()
#if !DEBUG
					.WriteTo.RollingFile($"{WorkingDirectory}/logs/{{Date}}.log", LogEventLevel.Information)
#endif
				)
				.Build();
	}
}
