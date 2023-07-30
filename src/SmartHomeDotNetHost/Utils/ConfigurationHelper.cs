using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartHomeDotNet.Utils;

public static class ConfigurationHelper
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration config, Action<ConfigurationBuilder> buildAction)
	{
		var builder = new ConfigurationBuilder(services, config);
		buildAction(builder);
		builder.Validate();

		return services;
	}

	public class ConfigurationBuilder
	{
		private readonly IServiceCollection _services;
		private readonly IConfiguration _config;
		private readonly IList<(string section, string error)> _errors = new List<(string, string)>();

		public ConfigurationBuilder(IServiceCollection services, IConfiguration config)
		{
			_services = services;
			_config = config;
		}

		public ConfigurationBuilder Add<T>(string sectionName)
			where T : class, IConfig, new()
		{
			var config = new T();
			_config.Bind(sectionName, config);
			_errors.AddRange(config.Validate().Select(error => (sectionName, error)));
			_services.AddSingleton<T>(config);

			return this;
		}

		public void Validate()
		{
			if (_errors.Count > 0)
			{
				throw new InvalidOperationException(
					"The configuration is invalid:" 
					+ Environment.NewLine
					+ string.Join(Environment.NewLine, _errors.Select(e => $"\t{e.section}: {e.error}")));
			}
		}
	}
}