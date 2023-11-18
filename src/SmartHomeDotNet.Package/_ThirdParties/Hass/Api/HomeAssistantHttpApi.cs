#nullable enable

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.Hass.Api;

/// <summary>
/// Represent the REST API of an <see cref="HomeAssistantHub"/>. (cf. <seealso cref="https://developers.home-assistant.io/docs/en/external_api_rest.html"/>).
/// </summary>
public class HomeAssistantHttpApi : IDisposable
{
	private readonly HttpClient _client;
	private readonly string _host;

	/// <summary>
	/// Creates a new instance given the uri and teh password of the REST API of a <see cref="HomeAssistantHub"/>.
	/// </summary>
	/// <param name="host">The uri of the REST API, eg. IP_ADDRESS:8123</param>
	/// <param name="apiToken">The API token of your home assistant hub</param>
	public HomeAssistantHttpApi(string host, string apiToken)
	{
		_host = host;

		var handler = new HttpClientHandler();
		handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true;
		_client = new HttpClient(handler)
		{
			DefaultRequestHeaders =
			{
				Authorization = new AuthenticationHeaderValue("Bearer", apiToken)
			},
			BaseAddress = new Uri($"https://{_host}/api")
		};
	}

	/// <summary>
	/// Call a service within a specific domain <seealso cref="https://developers.home-assistant.io/docs/en/external_api_rest.html#post-apiservicesltdomainltservice"/>.
	/// </summary>
	/// <param name="domain">The target domain of this call</param>
	/// <param name="service">The service to invoke</param>
	/// <param name="data">The parameters of the service (will be Json encoded), or `null` if no parameters</param>
	/// <param name="transition">The expected duration of the effect of the commend (for instance the fade in/out of a light)</param>
	/// <returns>An asynchronous <see cref="AsyncContextOperation"/>.</returns>
	public AsyncContextOperation CallService(string domain, string service, object data, TimeSpan? transition = null)
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
			var content = data == null
				? null
				: new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			var response = await _client.PostAsync($"https://{_host}/api/services/{domain}/{service}", content, ct);

			response.EnsureSuccessStatusCode();
		}
	}

	/// <summary>
	/// Send an **authenticated** raw request to home assistant
	/// </summary>
	/// <param name="request">The requests to send</param>
	/// <returns>The asynchronous response from the server.</returns>
	public Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken ct)
		=> _client.SendAsync(request, ct);

	/// <inheritdoc />
	public void Dispose()
		=> _client.Dispose();
}