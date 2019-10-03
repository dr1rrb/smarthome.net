using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SmartHomeDotNet.SmartHome.Devices;

namespace SmartHomeDotNet.IKEA.Devices
{
	public class TradfriRemote : Device
	{
		public DateTimeOffset LastSeen => DateTimeOffset.Parse(Raw.last_seen);

		public bool IsValidAction => Action != Actions.Unknown;

		public Actions Action { get; private set; }

		/// <inheritdoc />
		protected override void OnInit()
		{
			base.OnInit();

			if (TryGetValue("action", out var action))
			{
				Action = Parse(action);
			}
		}

		private Actions Parse(string action)
		{
			switch (action)
			{
				case "toggle": return Actions.Toggle;
				case "brightness_up_click": return Actions.BrightnessUpClick;
				case "brightness_up_hold": return Actions.BrightnessUpHold;
				case "brightness_up_release": return Actions.BrightnessUpRelease;
				case "brightness_down_click": return Actions.BrightnessDownClick;
				case "brightness_down_hold": return Actions.BrightnessDownHold;
				case "brightness_down_release": return Actions.BrightnessDownRelease;
				case "arrow_left_click": return Actions.ArrowLeftClick;
				case "arrow_left_hold": return Actions.ArrowLeftHold;
				case "arrow_left_release": return Actions.ArrowLeftRelease;
				case "arrow_right_click": return Actions.ArrowRightClick;
				case "arrow_right_hold": return Actions.ArrowRightHold;
				case "arrow_right_release": return Actions.ArrowRightRelease;
				default: return Actions.Unknown;
			}
		}

		public enum Actions
		{
			Unknown = 0,

			[JsonProperty("toggle")]
			Toggle,

			[JsonProperty("brightness_up_click")]
			BrightnessUpClick,
			[JsonProperty("brightness_up_hold")]
			BrightnessUpHold,
			[JsonProperty("brightness_up_release")]
			BrightnessUpRelease,

			[JsonProperty("brightness_down_click")]
			BrightnessDownClick,
			[JsonProperty("brightness_down_hold")]
			BrightnessDownHold,
			[JsonProperty("brightness_down_release")]
			BrightnessDownRelease,

			[JsonProperty("arrow_left_click")]
			ArrowLeftClick,
			[JsonProperty("arrow_left_hold")]
			ArrowLeftHold,
			[JsonProperty("arrow_left_release")]
			ArrowLeftRelease,

			[JsonProperty("arrow_right_click")]
			ArrowRightClick,
			[JsonProperty("arrow_right_hold")]
			ArrowRightHold,
			[JsonProperty("arrow_right_release")]
			ArrowRightRelease,
		}
	}
}
