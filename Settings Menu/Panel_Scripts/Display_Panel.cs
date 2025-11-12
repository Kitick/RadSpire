using Godot;

readonly record struct Resolution {
	public int Width { get; init; }
	public int Height { get; init; }

	public override string ToString() => $"{Width}x{Height}";
}

readonly record struct FPSOption {
	public int Value { get; init; }

	public override string ToString() => Value == 0 ? "Unlimited" : $"{Value} FPS";
}

namespace SettingsPanels {
	public partial class Display_Panel : VBoxContainer {
		// Paths
		private const string RESOLUTION = "Resolution/OptionButton";
		private const string FULLSCREEN = "Fullscreen/CheckBox";
		private const string VSYNC = "VSync/CheckBox";
		private const string BRIGHTNESS = "Brightness/HSlider";
		private const string FPS_CAP = "FPS_Cap/OptionButton";

		// Options
		private static readonly Resolution[] RESOLUTION_OPTIONS = [
			new Resolution { Width = 2560, Height = 1440 },
			new Resolution { Width = 1920, Height = 1080 },
			new Resolution { Width = 1280, Height = 720 },
		];

		private static readonly FPSOption[] FPS_OPTIONS = [
			new FPSOption { Value = 30 },
			new FPSOption { Value = 60 },
			new FPSOption { Value = 120 },
			new FPSOption { Value = 0 }, // Unlimited
		];

		public override void _Ready() {
			GetNode<OptionButton>(RESOLUTION).Populate(RESOLUTION_OPTIONS);
			GetNode<OptionButton>(FPS_CAP).Populate(FPS_OPTIONS);
			SetCallbacks();
		}

		private void SetCallbacks() {
			GetNode<OptionButton>(RESOLUTION).ItemSelected += OnResolutionSelected;
			GetNode<CheckBox>(FULLSCREEN).Toggled += SetFullscreen;
			GetNode<CheckBox>(VSYNC).Toggled += SetVSync;
			GetNode<HSlider>(BRIGHTNESS).ValueChanged += SetBrightness;
			GetNode<OptionButton>(FPS_CAP).ItemSelected += OnFPSCapSelected;
		}

		// Static setters
		private static void SetResolution(Resolution resolution) {
			GD.Print($"Setting resolution to: {resolution}");
			Vector2I size = new Vector2I(resolution.Width, resolution.Height);
			DisplayServer.WindowSetSize(size);
		}

		private static void SetFullscreen(bool isFullscreen) {
			GD.Print($"Setting fullscreen to: {isFullscreen}");
			var mode = isFullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
			DisplayServer.WindowSetMode(mode);
		}

		private static void SetVSync(bool isVSync) {
			GD.Print($"Setting VSync to: {isVSync}");
			ProjectSettings.SetSetting("display/window/vsync/use_vsync", isVSync);
			ProjectSettings.Save();
		}

		private static void SetBrightness(double value) {
			GD.Print($"Setting brightness to: {value}");
			ProjectSettings.SetSetting("display/window/brightness", (float)value);
			ProjectSettings.Save();
		}

		private static void SetFPS(FPSOption fps) {
			GD.Print($"Setting FPS cap to: {fps}");
			Engine.MaxFps = fps.Value;
		}

		// Callbacks
		private void OnResolutionSelected(long index) => SetResolution(RESOLUTION_OPTIONS[(int)index]);
		private void OnFPSCapSelected(long index) => SetFPS(FPS_OPTIONS[(int)index]);
	}
}