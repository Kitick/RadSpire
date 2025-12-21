using Core;
using Godot;
using Services;

namespace UI.Settings {
	public sealed partial class DisplayPanel : VBoxContainer, ISaveable<DisplaySettings> {
		private static readonly LogService Log = new(nameof(DisplayPanel), enabled: true);

		[Export] private OptionButton ResolutionOption = null!;
		[Export] private CheckBox FullscreenCheck = null!;
		[Export] private CheckBox VSyncCheck = null!;
		[Export] private HSlider BrightnessSlider = null!;
		[Export] private OptionButton FramerateOption = null!;

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
			ResolutionOption.ItemSelected += (index) => Resolution = Resolutions[(int) index];
			FullscreenCheck.Toggled += (value) => Fullscreen = value;
			VSyncCheck.Toggled += (value) => VSync = value;
			BrightnessSlider.ValueChanged += (value) => Brightness = (float) value;
			FramerateOption.ItemSelected += (index) => FPS = Framerates[(int) index];
		}

		// Resolution
		public static Resolution Resolution {
			get => Resolution.FromVector2I(DisplayServer.WindowGetSize());
			set {
				Log.Info($"Setting resolution to: {value}");
				DisplayServer.WindowSetSize(value.ToVector2I());
			}
		}

		// Fullscreen
		public static bool Fullscreen {
			get => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
			set {
				var mode = value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
				Log.Info($"Setting fullscreen to: {mode}");
				DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
			}
		}

		// VSYNC
		public static bool VSync {
			get => DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
			set {
				var mode = value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
				Log.Info($"Setting VSync to: {mode}");
				DisplayServer.WindowSetVsyncMode(mode);
			}
		}

		// Brightness
		public float Brightness {
			get;
			set;
		}

		// FPS Cap
		public static Framerate FPS {
			get => new Framerate { Value = Engine.MaxFps };
			set {
				Log.Info($"Setting FPS cap to: {value}");
				Engine.MaxFps = value.Value;
			}
		}

		// ISaveable implementation
		public DisplaySettings Export() {
			Resolution selectedResolution = Resolutions[ResolutionOption.Selected];
			Framerate selectedFPS = Framerates[FramerateOption.Selected];

			return new DisplaySettings {
				Resolution = selectedResolution,
				IsFullscreen = FullscreenCheck.ButtonPressed,
				IsVSyncEnabled = VSyncCheck.ButtonPressed,
				Brightness = (float) BrightnessSlider.Value,
				FPSCap = selectedFPS.Value
			};
		}

		public void Import(DisplaySettings data) {
			Resolution = data.Resolution;
			ResolutionOption.Select(data.Resolution);

			Fullscreen = data.IsFullscreen;
			FullscreenCheck.ButtonPressed = data.IsFullscreen;

			VSync = data.IsVSyncEnabled;
			VSyncCheck.ButtonPressed = data.IsVSyncEnabled;

			Brightness = data.Brightness;
			BrightnessSlider.Value = data.Brightness;

			FPS = new Framerate { Value = data.FPSCap };
			FramerateOption.Select(FPS);
		}
	}

	public readonly record struct Resolution {
		public int Width { get; init; }
		public int Height { get; init; }

		public readonly Vector2I ToVector2I() => new(Width, Height);
		public static Resolution FromVector2I(Vector2I size) => new Resolution { Width = size.X, Height = size.Y };

		public override string ToString() => $"{Height}p";
	}

	public readonly record struct Framerate {
		public int Value { get; init; }

		public override string ToString() => Value == 0 ? "Unlimited" : $"{Value} FPS";
	}

	public readonly record struct DisplaySettings : ISaveData {
		public Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}
}