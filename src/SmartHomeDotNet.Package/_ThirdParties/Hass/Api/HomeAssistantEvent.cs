using System;
using System.Linq;
using System.Text.Json;

namespace SmartHomeDotNet.Hass.Api
{
	public class HomeAssistantEvent
	{
		private readonly string _type;
		private readonly DateTimeOffset _time;
		private readonly string _data;

		public HomeAssistantEvent(string type, DateTimeOffset time, string data)
		{
			_type = type;
			_time = time;
			_data = data;
		}

		public string Type { get; }

		public DateTimeOffset Time { get; }

		public string RawData => _data;

		public T GetData<T>()
			=> JsonSerializer.Deserialize<T>(_data, HomeAssistantWebSocketApi.JsonReadOpts);
	}
}