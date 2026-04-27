namespace Settings.Interface;

using Godot;
using Root;
using Settings;

public sealed partial class ControllerPanel : VBoxContainer {

	[Export] private CheckBox VibrationCheckBox = null!;
	[Export] private HSlider DeadzoneSlider = null!;
	[Export] private HSlider ControllerSensitivitySlider = null!;
	[Export] private Button RemapButtonsButton = null!;

	public Control FirstControl => ControllerSensitivitySlider;

	public override void _Ready() {
		this.ValidateExports();
		DeadzoneSlider.ApplyBounds(ControllerSettings.Deadzone);
		ControllerSensitivitySlider.ApplyBounds(ControllerSettings.ControllerSensitivity);
		SetCallbacks();
	}

	private void SetCallbacks() {
		VibrationCheckBox.Toggled += ControllerSettings.Vibration.Apply;
		DeadzoneSlider.ValueChanged += (value) => ControllerSettings.Deadzone.Apply((float) value);
		ControllerSensitivitySlider.ValueChanged += (value) => ControllerSettings.ControllerSensitivity.Apply((float) value);
		RemapButtonsButton.Pressed += OnRemapButtonsPressed;
	}

	private void OnRemapButtonsPressed() {
		//Implementation Here
	}

	public void Refresh() {
		VibrationCheckBox.ButtonPressed = ControllerSettings.Vibration.Target;
		DeadzoneSlider.Value = ControllerSettings.Deadzone.Target;
		ControllerSensitivitySlider.Value = ControllerSettings.ControllerSensitivity.Target;
	}
}
