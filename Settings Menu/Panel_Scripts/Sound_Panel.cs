using System;
using Godot;

namespace SettingsPanels {
	public partial class Sound_Panel : VBoxContainer {
		private HSlider masterSlider = null!;
		private HSlider musicSlider = null!;
		private HSlider sfxSlider = null!;
		private CheckBox muteAllCheckBox = null!;
		private OptionButton outputDeviceOption = null!;

		private float prevMasterVolume;
		private float prevMusicVolume;
		private float prevSFXVolume;

		public override void _Ready() {
			GetComponents();
			SetCallBacks();

			masterSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"));
			musicSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
			sfxSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX"));

			masterSlider.Value = prevMasterVolume;
			musicSlider.Value = prevMusicVolume;
			sfxSlider.Value = prevSFXVolume;
		}

		private void GetComponents() {
			masterSlider = GetNode<HSlider>("Master_Volume/HSlider");
			musicSlider = GetNode<HSlider>("Music_Volume/HSlider");
			sfxSlider = GetNode<HSlider>("SFX_Volume/HSlider");
			muteAllCheckBox = GetNode<CheckBox>("Mute_All/CheckBox");
			outputDeviceOption = GetNode<OptionButton>("Output_Device/OptionButton");
		}

		private void SetCallBacks() {
			masterSlider.ValueChanged += OnMasterVolumeChanged;
			musicSlider.ValueChanged += OnMusicVolumeChanged;
			sfxSlider.ValueChanged += OnSFXVolumeChanged;
			muteAllCheckBox.Toggled += OnMuteAllToggled;

			var devices = AudioServer.GetOutputDeviceList();
			foreach(var device in devices) {
				outputDeviceOption.AddItem(device);
			}

			string currentDevice = AudioServer.OutputDevice;
			int index = Array.IndexOf(devices, currentDevice);
			if(index >= 0) {
				outputDeviceOption.Select(index);
			}

			outputDeviceOption.ItemSelected += OnOutputDeviceSelected;
		}

		private void OnMasterVolumeChanged(double value) {
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), (float)value);
			prevMasterVolume = (float)value;
		}

		private void OnMusicVolumeChanged(double value) {
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), (float)value);
			prevMusicVolume = (float)value;
		}

		private void OnSFXVolumeChanged(double value) {
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), (float)value);
			prevSFXVolume = (float)value;
		}

		private void OnMuteAllToggled(bool buttonPressed) {
			if(buttonPressed) {
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), -30);
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), -30);
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), -30);
			}
			else {
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), prevMasterVolume);
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), prevMusicVolume);
				AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), prevSFXVolume);
			}
		}

		private void OnOutputDeviceSelected(long index) {
			string selectedDevice = outputDeviceOption.GetItemText((int)index);
			AudioServer.OutputDevice = selectedDevice;
			GD.Print("Switched to output device: " + selectedDevice);
		}
	}
}