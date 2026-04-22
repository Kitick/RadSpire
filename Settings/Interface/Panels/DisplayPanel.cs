namespace Settings.Interface;

using Godot;
using Root;
using Settings;

public sealed partial class DisplayPanel : VBoxContainer {
	[ExportCategory("Display Settings")]
	[Export] private OptionButton ResolutionOption = null!;
	[Export] private CheckBox FullscreenCheck = null!;
	[Export] private CheckBox VSyncCheck = null!;
	[Export] private HSlider BrightnessSlider = null!;
	[Export] private OptionButton FramerateOption = null!;

	public Control[] Order => [
		ResolutionOption, FullscreenCheck, VSyncCheck, BrightnessSlider, FramerateOption
	];

	public override void _Ready() {
		this.ValidateExports();

		ResolutionOption.Populate(DisplaySettings.Resolution.Options);
		FramerateOption.Populate(DisplaySettings.MaxFps.Options);
		BrightnessSlider.ApplyBounds(DisplaySettings.Brightness);

		SetCallbacks();
	}

	private void SetCallbacks() {
		ResolutionOption.ItemSelected += (index) => DisplaySettings.Resolution.Apply(DisplaySettings.Resolution.Options[(int) index]);
		FullscreenCheck.Toggled += DisplaySettings.IsFullscreen.Apply;
		VSyncCheck.Toggled += DisplaySettings.IsVSync.Apply;
		BrightnessSlider.ValueChanged += (value) => DisplaySettings.Brightness.Apply((float) value);
		FramerateOption.ItemSelected += (index) => DisplaySettings.MaxFps.Apply(DisplaySettings.MaxFps.Options[(int) index]);
	}

	public void Refresh() {
		ResolutionOption.SelectItem(DisplaySettings.Resolution.Target);
		FullscreenCheck.ButtonPressed = DisplaySettings.IsFullscreen.Target;
		VSyncCheck.ButtonPressed = DisplaySettings.IsVSync.Target;
		BrightnessSlider.Value = DisplaySettings.Brightness.Target;
		FramerateOption.SelectItem(DisplaySettings.MaxFps.Target);
	}
}
