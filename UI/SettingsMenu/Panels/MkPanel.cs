using Godot;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class MkPanel : VBoxContainer {

		[Export] private HSlider MouseSenseSlider = null!;
		[Export] private CheckBox InvertedYAxisCheckBox = null!;
		[Export] private CheckBox RawInputCheckBox = null!;
		[Export] private Button RemapKeysButton = null!;

		public override void _Ready() {
			SetCallbacks();
		}

		private void SetCallbacks() {
			MouseSenseSlider.ValueChanged += (value) => MouseKeyboardSettings.MouseSensitivity.Apply((float) value);
			InvertedYAxisCheckBox.Toggled += MouseKeyboardSettings.InvertedYAxis.Apply;
			RawInputCheckBox.Toggled += MouseKeyboardSettings.RawInput.Apply;
			RemapKeysButton.Pressed += OnRemapKeysPressed;
		}

		private void OnRemapKeysPressed() {
			//Implementation Here
		}

		public void Refresh() {
			MouseSenseSlider.Value = MouseKeyboardSettings.MouseSensitivity.Target;
			InvertedYAxisCheckBox.ButtonPressed = MouseKeyboardSettings.InvertedYAxis.Target;
			RawInputCheckBox.ButtonPressed = MouseKeyboardSettings.RawInput.Target;
		}
	}
}
