using Godot;
using Services;

namespace UI.Settings {
	public sealed partial class ControllerPanel : VBoxContainer {

		[Export] private CheckBox EnableControllerCheckBox = null!;
		[Export] private CheckBox VibrationCheckBox = null!;
		[Export] private HSlider DeadzoneSlider = null!;
		[Export] private Button RemapButtonsButton = null!;

		public override void _Ready() {
			SetCallbacks();
		}

		// Set Callbacks
		public void SetCallbacks() {
			EnableControllerCheckBox.Toggled += OnEnableControllerCheckBox;
			VibrationCheckBox.Toggled += OnVibrationCheckbox;
			DeadzoneSlider.ValueChanged += OnDeadzoneChanged;
			RemapButtonsButton.Pressed += OnRemapButtonsPressed;
		}

		// Callbacks
		private void OnEnableControllerCheckBox(bool check) {
			//Implementation Here
		}

		private void OnVibrationCheckbox(bool check) {
			//Implementation Here
		}

		private void OnDeadzoneChanged(double value) {
			//Implementation Here
		}

		private void OnRemapButtonsPressed() {
			//Implementation Here
		}

		//ISaveable Implementation Goes Here

	}

	// Update as Needed
	public readonly record struct ControllerSettings : ISaveData {

	}
}
