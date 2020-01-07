using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;

namespace SmartHomeDotNet.Computer
{
	internal class SpeakersHelper
	{
		private readonly CoreAudioController _controller;

		public SpeakersHelper()
		{
			_controller = new CoreAudioController();
		}

		public IObservable<double> GetAndObserveVolume() => Observable.Defer(() => _controller
			.AudioDeviceChanged
			.Where(args => args.ChangedType == DeviceChangedType.DefaultChanged)
			.Select(args => args.Device)
			.StartWith(_controller.DefaultPlaybackDevice)
			.Select(device => device.VolumeChanged.Select(args => args.Volume).StartWith(device.Volume))
			.Switch()
			.DistinctUntilChanged()
		);

		public Task<bool> Mute(bool isMuted)
			=> _controller.DefaultPlaybackDevice.MuteAsync(isMuted);

		public async Task SetVolume(double volume)
			=> _controller.DefaultPlaybackDevice.Volume = volume;
	}
}