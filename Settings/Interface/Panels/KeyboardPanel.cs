namespace Settings.Interface;

using Godot;
using Root;
using Settings;

public sealed partial class KeyboardPanel : VBoxContainer {
	[Export] private HSlider MouseSenseSlider = null!;
	[Export] private CheckBox InvertedYAxisCheckBox = null!;
	[Export] private CheckBox RawInputCheckBox = null!;
	[Export] private Button RemapKeysButton = null!;

	public Control FirstControl => MouseSenseSlider;

	public override void _Ready() {
		this.ValidateExports();
		MouseSenseSlider.ApplyBounds(KeyboardSettings.MouseSensitivity);
		SetCallbacks();
	}

	private void SetCallbacks() {
		MouseSenseSlider.ValueChanged += (value) => KeyboardSettings.MouseSensitivity.Apply((float) value);
		InvertedYAxisCheckBox.Toggled += KeyboardSettings.InvertedYAxis.Apply;
		RawInputCheckBox.Toggled += KeyboardSettings.RawInput.Apply;
		RemapKeysButton.Pressed += OnRemapKeysPressed;
	}

	private void OnRemapKeysPressed() {
		//Implementation Here
	}

	public void Refresh() {
		MouseSenseSlider.Value = KeyboardSettings.MouseSensitivity.Target;
		InvertedYAxisCheckBox.ButtonPressed = KeyboardSettings.InvertedYAxis.Target;
		RawInputCheckBox.ButtonPressed = KeyboardSettings.RawInput.Target;
	}
}
