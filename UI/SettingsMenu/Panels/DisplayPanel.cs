namespace UI.Settings;

using Godot;
using Root;
using Services.Settings;

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

	// Options
	private static readonly Resolution[] Resolutions = [
		new Resolution { Width = 1280, Height = 720 },
			new Resolution { Width = 1600, Height = 900 },
			new Resolution { Width = 1920, Height = 1080 },
			new Resolution { Width = 2560, Height = 1440 },
			new Resolution { Width = 3840, Height = 2160 },
		];

	private static readonly Framerate[] Framerates = [
		new Framerate { Value = 0 }, //Unlimited
			new Framerate { Value = 30 },
			new Framerate { Value = 60 },
			new Framerate { Value = 120 },
			new Framerate { Value = 144 },
			new Framerate { Value = 165 },
		];

	// Main
	public override void _Ready() {
		this.ValidateExports();

		ResolutionOption.Populate(Resolutions);
		FramerateOption.Populate(Framerates);

		SetCallbacks();
	}

	// Callbacks
	private void SetCallbacks() {
		ResolutionOption.ItemSelected += (index) => DisplaySettings.Resolution.Apply(Resolutions[(int) index]);
		FullscreenCheck.Toggled += DisplaySettings.IsFullscreen.Apply;
		VSyncCheck.Toggled += DisplaySettings.IsVSync.Apply;
		BrightnessSlider.ValueChanged += (value) => DisplaySettings.Brightness.Apply((float) value);
		FramerateOption.ItemSelected += (index) => DisplaySettings.MaxFps.Apply(Framerates[(int) index].Value);
	}

	public void Refresh() {
		ResolutionOption.SelectItem(DisplaySettings.Resolution.Target);
		FullscreenCheck.ButtonPressed = DisplaySettings.IsFullscreen.Target;
		VSyncCheck.ButtonPressed = DisplaySettings.IsVSync.Target;
		BrightnessSlider.Value = DisplaySettings.Brightness.Target;
		FramerateOption.SelectItem(new Framerate { Value = DisplaySettings.MaxFps.Target });
	}
}
