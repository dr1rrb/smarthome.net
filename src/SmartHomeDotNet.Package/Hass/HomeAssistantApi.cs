using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartHomeDotNet.Logging;

namespace SmartHomeDotNet.Hass
{
	public class HomeAssistantApi
	{
		private readonly HttpClient _api;
		private readonly string _baseUri;

		public HomeAssistantApi(string uri, string apiPassword)
		{
			_baseUri = uri;

			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true;
			_api = new HttpClient(handler)
			{
				DefaultRequestHeaders =
				{
					{"x-ha-access", apiPassword}
				}
			};
		}

		public Call Execute(string domain, string service, object parameters)
		{
			var content = parameters == null
				? null
				: new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

			return new Call(_api, $"https://{_baseUri}/api/services/{domain}/{service}", content);
		}

		public class Call : INotifyCompletion
		{
			private readonly CancellationTokenSource _ct = new CancellationTokenSource();
			private readonly Task _task;

			public Call(HttpClient target, string requestUri, HttpContent payload)
			{
				_task = Start(target, requestUri, payload);
			}

			private async Task Start(HttpClient target, string requestUri, HttpContent payload)
			{
				try
				{
					var response = await target.PostAsync(requestUri, payload, _ct.Token);
					response.EnsureSuccessStatusCode();
				}
				catch (Exception e)
				{
					this.Log().Error("Failed to send message to HA", e);
				}
			}

			public void LinkTo(CancellationToken ct)
			{
				ct.Register(_ct.Cancel);
			}

			public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

			public bool IsCompleted => GetAwaiter().IsCompleted;

			public void GetResult() => GetAwaiter().GetResult();

			public void OnCompleted(Action continuation) => GetAwaiter().OnCompleted(continuation);
		}
	}
}