using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
		public HomeAssistantApi(string host, string apiToken)
		{
			_host = host;

			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true;
			_client = new HttpClient(handler)
			{
				DefaultRequestHeaders =
				{
					Authorization = new AuthenticationHeaderValue("Bearer", apiToken)
				}
			};
		}

		/// <summary>
		/// Executes an API call
		/// </summary>
		/// <param name="domain">The target domain of this call</param>
		/// <param name="service">The service to invoke</param>
		/// <param name="parameters">The parameters of the service (will be Json encoded), or `null` if no parameters</param>
		/// <returns>An asynchronous <see cref="AsyncContextOperation"/>.</returns>
		public AsyncContextOperation Execute(string domain, string service, object parameters, TimeSpan? transition = null)
		{
			if (transition.GetValueOrDefault() > TimeSpan.Zero)
			{
				return AsyncContextOperation.StartNew(Send, ct => Task.Delay(transition.Value, ct));
			}
			else
			{
				return AsyncContextOperation.StartNew(Send);
			}

			async Task Send(CancellationToken ct)
			{
				var content = parameters == null
					? null
					: new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
				var response = await _client.PostAsync($"https://{_host}/api/services/{domain}/{service}", content, ct);

				response.EnsureSuccessStatusCode();
			}
		}
	}
}