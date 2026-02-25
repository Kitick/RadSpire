using Core;
using Godot;
using Services;
using Services.Settings;

namespace UI.Settings {
	public sealed partial class DisplayPanel : VBoxContainer, ISaveable<DisplayData> {
		private static readonly LogService Log = new(nameof(DisplayPanel), enabled: true);

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
			ResolutionOption.ItemSelected += (index) => DisplaySettings.Resolution = Resolutions[(int) index];
			FullscreenCheck.Toggled += (value) => DisplaySettings.IsFullscreen = value;
			VSyncCheck.Toggled += (value) => DisplaySettings.IsVSync = value;
			BrightnessSlider.ValueChanged += (value) => DisplaySettings.Brightness = (float) value;
			FramerateOption.ItemSelected += (index) => DisplaySettings.MaxFps = Framerates[(int) index].Value;
		}

		// ISaveable implementation
		public DisplayData Export() {
			Resolution selectedResolution = Resolutions[ResolutionOption.Selected];
			Framerate selectedFPS = Framerates[FramerateOption.Selected];

			return new DisplayData {
				Resolution = selectedResolution,
				IsFullscreen = FullscreenCheck.ButtonPressed,
				IsVSyncEnabled = VSyncCheck.ButtonPressed,
				Brightness = (float) BrightnessSlider.Value,
				FPSCap = selectedFPS.Value
			};
		}

		public void Import(DisplayData data) {
			ResolutionOption.Select(data.Resolution);
			FullscreenCheck.ButtonPressed = data.IsFullscreen;
			VSyncCheck.ButtonPressed = data.IsVSyncEnabled;
			BrightnessSlider.Value = data.Brightness;
			FramerateOption.Select(new Framerate { Value = data.FPSCap });
		}
	}

	public readonly record struct DisplayData : ISaveData {
		public Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}
}