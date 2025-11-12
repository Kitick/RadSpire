using System;
using System.Collections.Generic;
using Godot;

namespace SettingsPanels {
	public enum AudioBus { Master, Music, SFX }

	public static class Extensions {
		public static string GetName(this AudioBus bus) =>
			Sound_Panel.BusNames[bus];

		private static int GetIndex(this AudioBus bus) =>
			AudioServer.GetBusIndex(bus.GetName());

		public static float GetVolume(this AudioBus bus) =>
			AudioServer.GetBusVolumeLinear(bus.GetIndex()) * 100f;

		public static void SetVolume(this AudioBus bus, float volume) {
			GD.Print($"Set {bus.GetName()} volume to {volume}{(bus.IsMuted() ? " (muted)" : "")}");
			AudioServer.SetBusVolumeLinear(bus.GetIndex(), volume / 100f);
		}

		public static bool IsMuted(this AudioBus bus) =>
			AudioServer.IsBusMute(bus.GetIndex());

		public static void SetMuted(this AudioBus bus, bool isMuted) {
			GD.Print($"{(isMuted ? "Muted" : "Unmuted")} {bus.GetName()} bus");
			AudioServer.SetBusMute(bus.GetIndex(), isMuted);
		}
	}

	public partial class Sound_Panel : VBoxContainer {
		// Paths
		private const string MASTER_SLIDER = "Master_Volume/HSlider";
		private const string MUSIC_SLIDER = "Music_Volume/HSlider";
		private const string SFX_SLIDER = "SFX_Volume/HSlider";
		private const string MUTE_ALL_CHECKBOX = "Mute_All/CheckBox";
		private const string OUTPUT_DEVICE = "Output_Device/OptionButton";

		// Mapping enum to bus names
		public static readonly Dictionary<AudioBus, string> BusNames = new() {
			{ AudioBus.Master, "Master" },
			{ AudioBus.Music, "Music" },
			{ AudioBus.SFX, "SFX" }
		};

		// Mapping enum to slider paths
		private static readonly Dictionary<AudioBus, string> BusToSlider = new() {
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

		private HSlider GetSlider(AudioBus bus) => GetNode<HSlider>(BusToSlider[bus]);

		private float GetSliderValue(AudioBus bus) => (float)GetSlider(bus).Value;
		private void SetSliderValue(AudioBus bus, float value) => GetSlider(bus).Value = value;

		// Sets the initial slider values based on current volumes
		private void LoadCurrentVolumes() {
			foreach(var bus in BusNames.Keys) {
				SetSliderValue(bus, bus.GetVolume());
			}
		}

		private static void SetMuteAll(bool isMuted) {
			foreach(var bus in BusNames.Keys) {
				bus.SetMuted(isMuted);
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
			if(index == -1) { return false; }

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

		// Callbacks
		private void OnMasterVolumeChanged(double value) => AudioBus.Master.SetVolume((float)value);
		private void OnMusicVolumeChanged(double value) => AudioBus.Music.SetVolume((float)value);
		private void OnSFXVolumeChanged(double value) => AudioBus.SFX.SetVolume((float)value);
		private void OnMuteAllToggled(bool isMuted) => SetMuteAll(isMuted);

		private void OnOutputDeviceSelected(long index) {
			string selectedDevice = OutputDeviceOption.GetItemText((int)index);
			SetOutputDevice(selectedDevice);
		}
	}
}