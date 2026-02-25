using Godot;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class ControllerPanel : VBoxContainer {

		[Export] private CheckBox EnableControllerCheckBox = null!;
		[Export] private CheckBox VibrationCheckBox = null!;
		[Export] private HSlider DeadzoneSlider = null!;
		[Export] private Button RemapButtonsButton = null!;

		public override void _Ready() {
			SetCallbacks();
		}

		private void SetCallbacks() {
			EnableControllerCheckBox.Toggled += ControllerSettings.EnableController.Apply;
			VibrationCheckBox.Toggled += ControllerSettings.Vibration.Apply;
			DeadzoneSlider.ValueChanged += (value) => ControllerSettings.Deadzone.Apply((float) value);
			RemapButtonsButton.Pressed += OnRemapButtonsPressed;
		}

		private void OnRemapButtonsPressed() {
			//Implementation Here
		}

		public void Refresh() {
			EnableControllerCheckBox.ButtonPressed = ControllerSettings.EnableController.Target;
			VibrationCheckBox.ButtonPressed = ControllerSettings.Vibration.Target;
			DeadzoneSlider.Value = ControllerSettings.Deadzone.Target;
		}
	}
}
