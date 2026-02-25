using Core;
using Godot;
using Services;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class SoundPanel : VBoxContainer, ISaveable<AudioData> {
		private static readonly LogService Log = new(nameof(SoundPanel), enabled: true);

		[Export] private HSlider MasterSlider = null!;
		[Export] private HSlider MusicSlider = null!;
		[Export] private HSlider SFXSlider = null!;
		[Export] private CheckBox MuteAllCheckBox = null!;
		[Export] private OptionButton OutputDeviceOption = null!;

		public override void _Ready() {
			SetCallbacks();
			OutputDeviceOption.Populate(AudioServer.GetOutputDeviceList());
		}

		private void SetCallbacks() {
			MasterSlider.ValueChanged += OnMasterVolumeChanged;
			MusicSlider.ValueChanged += OnMusicVolumeChanged;
			SFXSlider.ValueChanged += OnSFXVolumeChanged;
			MuteAllCheckBox.Toggled += OnMuteAllToggled;
			OutputDeviceOption.ItemSelected += OnOutputDeviceSelected;
		}

		// Callbacks
		private void OnMasterVolumeChanged(double value) => AudioSettings.MasterVolume = (int) value;
		private void OnMusicVolumeChanged(double value) => AudioSettings.MusicVolume = (int) value;
		private void OnSFXVolumeChanged(double value) => AudioSettings.SFXVolume = (int) value;
		private void OnMuteAllToggled(bool isMuted) => AudioSettings.IsMuted = isMuted;
		private void OnOutputDeviceSelected(long index) => AudioSettings.OutputDevice = OutputDeviceOption.GetItemText((int) index);

		public AudioData Export() => new AudioData {
			MasterVolume = (int) MasterSlider.Value,
			MusicVolume = (int) MusicSlider.Value,
			SFXVolume = (int) SFXSlider.Value,
			IsMuted = MuteAllCheckBox.ButtonPressed,
			OutputDevice = OutputDeviceOption.GetItemText(OutputDeviceOption.Selected),
		};

		public void Import(AudioData data) {
			MasterSlider.Value = data.MasterVolume;
			MusicSlider.Value = data.MusicVolume;
			SFXSlider.Value = data.SFXVolume;
			MuteAllCheckBox.ButtonPressed = data.IsMuted;
			OutputDeviceOption.Select(data.OutputDevice);
		}
	}

	public readonly record struct AudioData : ISaveData {
		public int MasterVolume { get; init; }
		public int MusicVolume { get; init; }
		public int SFXVolume { get; init; }
		public bool IsMuted { get; init; }
		public string OutputDevice { get; init; }
	}
}