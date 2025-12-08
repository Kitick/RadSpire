using System;
using System.Collections.Generic;
using Core;
using Godot;
using SaveSystem;

namespace Settings {
	public enum AudioBus { Master, Music, SFX }

	public sealed partial class SoundPanel : VBoxContainer, ISaveable<SoundSettings> {
		private static readonly Logger Log = new(nameof(SoundPanel), enabled: true);

		[Export] private HSlider MasterSlider = null!;
		[Export] private HSlider MusicSlider = null!;
		[Export] private HSlider SFXSlider = null!;
		[Export] private CheckBox MuteAllCheckBox = null!;
		[Export] private OptionButton OutputDeviceOption = null!;

		public override void _Ready() {
			LoadCurrentVolumes();
			SetCallbacks();

			// Configure output devices
			OutputDeviceOption.Populate(AudioServer.GetOutputDeviceList());
			OutputDeviceOption.Select(AudioServer.OutputDevice);
		}

		private void SetCallbacks() {
			MasterSlider.ValueChanged += OnMasterVolumeChanged;
			MusicSlider.ValueChanged += OnMusicVolumeChanged;
			SFXSlider.ValueChanged += OnSFXVolumeChanged;
			MuteAllCheckBox.Toggled += OnMuteAllToggled;
			OutputDeviceOption.ItemSelected += OnOutputDeviceSelected;
		}

		private HSlider GetSlider(AudioBus bus) => bus switch {
			AudioBus.Master => MasterSlider,
			AudioBus.Music => MusicSlider,
			AudioBus.SFX => SFXSlider,
			_ => throw new ArgumentException($"Unknown bus: {bus}")
		};

		private float GetSliderValue(AudioBus bus) => (float) GetSlider(bus).Value;
		private void SetSliderValue(AudioBus bus, float value) => GetSlider(bus).Value = value;

		// Sets the initial slider values based on current volumes
		private void LoadCurrentVolumes() {
			foreach(var bus in AudioBusExtensions.GetAllNames()) {
				SetSliderValue(bus, bus.GetVolume());
			}
		}

		private static void SetMuteAll(bool isMuted) {
			foreach(var bus in AudioBusExtensions.GetAllNames()) {
				bus.SetMuted(isMuted);
			}
		}

		// Setters
		private static bool SetOutputDevice(string deviceName) {
			var devices = AudioServer.GetOutputDeviceList();
			int index = Array.IndexOf(devices, deviceName);

			if(index == -1) { return false; }

			Log.Info($"Switched to output device: {deviceName}");
			AudioServer.OutputDevice = deviceName;

			return true;
		}

		// Callbacks
		private void OnMasterVolumeChanged(double value) => AudioBus.Master.SetVolume(value);
		private void OnMusicVolumeChanged(double value) => AudioBus.Music.SetVolume(value);
		private void OnSFXVolumeChanged(double value) => AudioBus.SFX.SetVolume(value);
		private void OnMuteAllToggled(bool isMuted) => SetMuteAll(isMuted);

		private void OnOutputDeviceSelected(long index) {
			string selectedDevice = OutputDeviceOption.GetItemText((int) index);
			SetOutputDevice(selectedDevice);
		}

		public SoundSettings Serialize() => new SoundSettings {
			MasterVolume = AudioBus.Master.GetVolume(),
			MusicVolume = AudioBus.Music.GetVolume(),
			SFXVolume = AudioBus.SFX.GetVolume(),
			IsMuted = AudioBus.Master.IsMuted(),
			OutputDevice = AudioServer.OutputDevice,
		};

		public void Deserialize(in SoundSettings data) {
			AudioBus.Master.SetVolume(data.MasterVolume);
			AudioBus.Music.SetVolume(data.MusicVolume);
			AudioBus.SFX.SetVolume(data.SFXVolume);
			AudioBus.Master.SetMuted(data.IsMuted);
			SetOutputDevice(data.OutputDevice);

			MasterSlider.Value = data.MasterVolume;
			MusicSlider.Value = data.MusicVolume;
			SFXSlider.Value = data.SFXVolume;
			MuteAllCheckBox.ButtonPressed = data.IsMuted;

			OutputDeviceOption.Select(data.OutputDevice);
		}
	}

	// AudioBus extension methods for volume and mute controls
	public static class AudioBusExtensions {
		public static AudioBus[] GetAllNames() => Enum.GetValues<AudioBus>();
		public static string GetName(this AudioBus bus) => bus.ToString();

		private static int GetIndex(this AudioBus bus) =>
			AudioServer.GetBusIndex(bus.GetName());

		public static int GetVolume(this AudioBus bus) =>
			(int) Math.Round(AudioServer.GetBusVolumeLinear(bus.GetIndex()) * 100f);

		public static void SetVolume(this AudioBus bus, double volume) =>
			bus.SetVolume((int) Math.Round(volume));

		public static void SetVolume(this AudioBus bus, int volume) {
			// Log.Info($"Set {bus.GetName()} volume to {volume}{(bus.IsMuted() ? " (muted)" : "")}");
			AudioServer.SetBusVolumeLinear(bus.GetIndex(), volume / 100f);
		}

		public static bool IsMuted(this AudioBus bus) =>
			AudioServer.IsBusMute(bus.GetIndex());

		public static void SetMuted(this AudioBus bus, bool isMuted) {
			// Log.Info($"{(isMuted ? "Muted" : "Unmuted")} {bus.GetName()} bus");
			AudioServer.SetBusMute(bus.GetIndex(), isMuted);
		}
	}
}

namespace SaveSystem {
	public readonly record struct SoundSettings : ISaveData {
		public int MasterVolume { get; init; }
		public int MusicVolume { get; init; }
		public int SFXVolume { get; init; }
		public bool IsMuted { get; init; }
		public string OutputDevice { get; init; }
	}
}