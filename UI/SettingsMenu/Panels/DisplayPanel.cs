using Core;
using Godot;
using Services.Settings;

namespace UI.Settings {
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
			ResolutionOption.Populate(Resolutions);
			FramerateOption.Populate(Framerates);

			SetCallbacks();
		}

		// Callbacks
		private void SetCallbacks() {
			ResolutionOption.ItemSelected += (index) => DisplaySettings.Resolution.Target = Resolutions[(int) index];
			FullscreenCheck.Toggled += (value) => DisplaySettings.IsFullscreen.Target = value;
			VSyncCheck.Toggled += (value) => DisplaySettings.IsVSync.Target = value;
			BrightnessSlider.ValueChanged += (value) => DisplaySettings.Brightness.Target = (float) value;
			FramerateOption.ItemSelected += (index) => DisplaySettings.MaxFps.Target = Framerates[(int) index].Value;
		}

		public void Refresh() {
			ResolutionOption.Select(DisplaySettings.Resolution.Target);
			FullscreenCheck.ButtonPressed = DisplaySettings.IsFullscreen.Target;
			VSyncCheck.ButtonPressed = DisplaySettings.IsVSync.Target;
			BrightnessSlider.Value = DisplaySettings.Brightness.Target;
			FramerateOption.Select(new Framerate { Value = DisplaySettings.MaxFps.Target });
		}
	}
}