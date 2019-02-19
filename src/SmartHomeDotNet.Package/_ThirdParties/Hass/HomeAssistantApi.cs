using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass
{
	/// <summary>
	/// Represent the REST API of an <see cref="HomeAssistantHub"/>. (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_rest.html"/>).
	/// </summary>
	public class HomeAssistantApi
	{
		private readonly HttpClient _client;
		private readonly string _host;

		/// <summary>
		/// Creates a new instance given the uri and teh password of the REST API of a <see cref="HomeAssistantHub"/>.
		/// </summary>
		/// <param name="host">The uri of the REST API, eg. IP_ADDRESS:8123</param>
		/// <param name="apiPassword">The API password configured in your configuration.yml.</param>
		public HomeAssistantApi(string host, string apiPassword)
		{
			_host = host;

			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true;
			_client = new HttpClient(handler)
			{
				DefaultRequestHeaders =
				{
					{"x-ha-access", apiPassword}
				}
			};
		}

		/// <summary>
		/// Executes an API call
		/// </summary>
		/// <param name="domain">The target domain of this call</param>
		/// <param name="service">The service to invoke</param>
		/// <param name="parameters">The parameters of the service (will be Json encoded), or `null` if no parameters</param>
		/// <returns>An asynchronous <see cref="ApiCall"/>.</returns>
		public ApiCall Execute(string domain, string service, object parameters)
		{
			var content = parameters == null
				? null
				: new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

			return new ApiCall(_client, $"https://{_host}/api/services/{domain}/{service}", content);
		}
	}
}