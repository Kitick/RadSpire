using System;
using System.Collections.Generic;
using Godot;

namespace SettingsPanels {
	public enum AudioBus { Master, Music, SFX }

	public partial class Sound_Panel : VBoxContainer {
		// Paths
		private const string MASTER_SLIDER = "Master_Volume/HSlider";
		private const string MUSIC_SLIDER = "Music_Volume/HSlider";
		private const string SFX_SLIDER = "SFX_Volume/HSlider";
		private const string MUTE_ALL_CHECKBOX = "Mute_All/CheckBox";
		private const string OUTPUT_DEVICE = "Output_Device/OptionButton";

		// Mute threshold
		private const float MUTE_VOLUME = -30f;

		// Mapping enum to bus names
		private readonly Dictionary<AudioBus, string> BusNames = new() {
			{ AudioBus.Master, "Master" },
			{ AudioBus.Music, "Music" },
			{ AudioBus.SFX, "SFX" }
		};

		// Store previous volumes for mute/unmute
		private readonly Dictionary<AudioBus, float> PrevVolumes = new() {
			{ AudioBus.Master, 0f },
			{ AudioBus.Music, 0f },
			{ AudioBus.SFX, 0f }
		};

		private readonly Dictionary<AudioBus, string> BusToSlider = new() {
			{ AudioBus.Master, MASTER_SLIDER },
			{ AudioBus.Music, MUSIC_SLIDER },
			{ AudioBus.SFX, SFX_SLIDER }
		};

		private OptionButton OutputDeviceOption = null!;

		public override void _Ready() {
			GetComponents();
			LoadCurrentVolumes();
			PopulateOutputDevices();
			SelectOutputDevice(AudioServer.OutputDevice);
			SetCallbacks();
		}

		private void GetComponents() {
			OutputDeviceOption = GetNode<OptionButton>(OUTPUT_DEVICE);
		}

		private void SetCallbacks() {
			GetNode<HSlider>(MASTER_SLIDER).ValueChanged += OnMasterVolumeChanged;
			GetNode<HSlider>(MUSIC_SLIDER).ValueChanged += OnMusicVolumeChanged;
			GetNode<HSlider>(SFX_SLIDER).ValueChanged += OnSFXVolumeChanged;
			GetNode<CheckBox>(MUTE_ALL_CHECKBOX).Toggled += OnMuteAllToggled;
			OutputDeviceOption.ItemSelected += OnOutputDeviceSelected;
		}

		// Sets the initial slider values based on current volumes
		private void LoadCurrentVolumes() {
			foreach(var (bus, name) in BusNames) {
				float volumeDb = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(name));

				string sliderPath = BusToSlider[bus];
				PrevVolumes[bus] = volumeDb;

				GetNode<HSlider>(sliderPath).Value = volumeDb;
			}
		}

		// Populates output device option button
		private void PopulateOutputDevices() {
			var devices = AudioServer.GetOutputDeviceList();

			OutputDeviceOption.Clear();
			foreach(var device in devices) {
				OutputDeviceOption.AddItem(device);
			}
		}

		private static int GetOutputDeviceIndex(string deviceName) {
			var devices = AudioServer.GetOutputDeviceList();
			return Array.IndexOf(devices, deviceName);
		}

		// Selects the given output device in the option button
		private bool SelectOutputDevice(string deviceName) {
			int index = GetOutputDeviceIndex(deviceName);
			if(index == -1){ return false; }

			OutputDeviceOption.Select(index);
			return true;
		}

		// Setters
		private static bool SetOutputDevice(string deviceName) {
			if(GetOutputDeviceIndex(deviceName) == -1) { return false; }

			GD.Print($"Switched to output device: {deviceName}");
			AudioServer.OutputDevice = deviceName;

			return true;
		}

		private void SetBusVolume(AudioBus bus, float volumeDb) {
			string name = BusNames[bus];
			GD.Print($"Setting volume of bus '{name}' to {volumeDb} dB");

			int busIndex = AudioServer.GetBusIndex(name);
			AudioServer.SetBusVolumeDb(busIndex, volumeDb);
		}

		private void SetVolume(AudioBus bus, float volumeDb) {
			SetBusVolume(bus, volumeDb);
			PrevVolumes[bus] = volumeDb;
		}

		private void SetAllMute(bool isMuted) {
			foreach(var bus in BusNames.Keys) {
				if(isMuted) {
					SetBusVolume(bus, MUTE_VOLUME);
				}
				else {
					SetBusVolume(bus, PrevVolumes[bus]);
				}
			}
		}

		// Callbacks
		private void OnMasterVolumeChanged(double value) => SetVolume(AudioBus.Master, (float)value);
		private void OnMusicVolumeChanged(double value) => SetVolume(AudioBus.Music, (float)value);
		private void OnSFXVolumeChanged(double value) => SetVolume(AudioBus.SFX, (float)value);
		private void OnMuteAllToggled(bool isMuted) => SetAllMute(isMuted);

		private void OnOutputDeviceSelected(long index) {
			string selectedDevice = OutputDeviceOption.GetItemText((int)index);
			SetOutputDevice(selectedDevice);
		}
	}
}