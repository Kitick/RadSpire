namespace Settings.Interface;

using Godot;
using Root;
using Settings;

public sealed partial class SoundPanel : VBoxContainer {
	[ExportCategory("Audio Settings")]
	[Export] private HSlider MasterSlider = null!;
	[Export] private HSlider MusicSlider = null!;
	[Export] private HSlider SFXSlider = null!;
	[Export] private CheckBox MuteAllCheckBox = null!;
	[Export] private OptionButton OutputDeviceOption = null!;

	public override void _Ready() {
		this.ValidateExports();

		MasterSlider.ApplyBounds(AudioSettings.MasterVolume);
		MusicSlider.ApplyBounds(AudioSettings.MusicVolume);
		SFXSlider.ApplyBounds(AudioSettings.SFXVolume);

		SetCallbacks();
		OutputDeviceOption.Populate(AudioServer.GetOutputDeviceList());
	}

	private void SetCallbacks() {
		MasterSlider.ValueChanged += (value) => AudioSettings.MasterVolume.Apply((int) value);
		MusicSlider.ValueChanged += (value) => AudioSettings.MusicVolume.Apply((int) value);
		SFXSlider.ValueChanged += (value) => AudioSettings.SFXVolume.Apply((int) value);
		MuteAllCheckBox.Toggled += AudioSettings.IsMuted.Apply;
		OutputDeviceOption.ItemSelected += (index) => AudioSettings.OutputDevice.Apply(OutputDeviceOption.GetItemText((int) index));
	}

	public void Refresh() {
		MasterSlider.Value = AudioSettings.MasterVolume.Target;
		MusicSlider.Value = AudioSettings.MusicVolume.Target;
		SFXSlider.Value = AudioSettings.SFXVolume.Target;
		MuteAllCheckBox.ButtonPressed = AudioSettings.IsMuted.Target;
		OutputDeviceOption.SelectItem(AudioSettings.OutputDevice.Target);
	}
}
