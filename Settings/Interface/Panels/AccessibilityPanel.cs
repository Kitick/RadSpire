namespace Settings.Interface;

using Godot;
using Root;
using Settings;

public sealed partial class AccessibilityPanel : VBoxContainer {
	[ExportCategory("Accessibility Settings")]
	[Export] private CheckBox SubtitlesCheckBox = null!;
	[Export] private HSlider SubtitleSizeSlider = null!;
	[Export] private OptionButton ColorblindModeOption = null!;
	[Export] private CheckBox TextToSpeechCheckBox = null!;
	[Export] private CheckBox HighContrastUICheckBox = null!;

	public override void _Ready() {
		this.ValidateExports();
		SetCallbacks();
	}

	private void SetCallbacks() {
		SubtitlesCheckBox.Toggled += AccessibilitySettings.Subtitles.Apply;
		SubtitleSizeSlider.ValueChanged += (value) => AccessibilitySettings.SubtitleSize.Apply((float) value);
		ColorblindModeOption.ItemSelected += (index) => AccessibilitySettings.ColorblindMode.Apply(ColorblindModeOption.GetItemText((int) index));
		TextToSpeechCheckBox.Toggled += AccessibilitySettings.TextToSpeech.Apply;
		HighContrastUICheckBox.Toggled += AccessibilitySettings.HighContrastUI.Apply;
	}

	public void Refresh() {
		SubtitlesCheckBox.ButtonPressed = AccessibilitySettings.Subtitles.Target;
		SubtitleSizeSlider.Value = AccessibilitySettings.SubtitleSize.Target;
		ColorblindModeOption.SelectItem(AccessibilitySettings.ColorblindMode.Target);
		TextToSpeechCheckBox.ButtonPressed = AccessibilitySettings.TextToSpeech.Target;
		HighContrastUICheckBox.ButtonPressed = AccessibilitySettings.HighContrastUI.Target;
	}
}
