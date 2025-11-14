using Core;
using Godot;
using SaveSystem;

namespace SettingsPanels {
	public readonly record struct Resolution {
		public int Width { get; init; }
		public int Height { get; init; }

		public override string ToString() => $"{Width}x{Height}";
	}

	public readonly record struct FPS {
		public int Value { get; init; }

		public override string ToString() => Value == 0 ? "Unlimited" : $"{Value} FPS";
	}

	public partial class Display_Panel : VBoxContainer, ISaveable<DisplaySettings> {
		//Node Paths
		private const string RESOLUTION = "Resolution/OptionButton";
		private const string FULLSCREEN = "Fullscreen/CheckBox";
		private const string VSYNC = "VSync/CheckBox";
		private const string BRIGHTNESS = "Brightness/HSlider";
		private const string WORLD_ENVIRONMENT = "res://World Environment/World_Environment.tscn";
		private const string FPS_CAP = "FPS_Cap/OptionButton";

		//Options
		private static readonly Resolution[] RESOLUTION_OPTIONS = [
			new Resolution { Width = 2560, Height = 1440 },
			new Resolution { Width = 1920, Height = 1080 },
			new Resolution { Width = 1280, Height = 720 },
		];

		private static readonly FPS[] FPS_OPTIONS = [
			new FPS { Value = 30 },
			new FPS { Value = 60 },
			new FPS { Value = 120 },
			new FPS { Value = 0 }, //Unlimited
		];

		//Main
		public override void _Ready() {
			GetNode<OptionButton>(RESOLUTION).Populate(RESOLUTION_OPTIONS);
			GetNode<OptionButton>(FPS_CAP).Populate(FPS_OPTIONS);

			SetCallbacks();
		}

		private void SetCallbacks() {
			GetNode<OptionButton>(RESOLUTION).ItemSelected += OnResolutionSelected;
			GetNode<CheckBox>(FULLSCREEN).Toggled += SetFullscreen;
			GetNode<CheckBox>(VSYNC).Toggled += SetVSync;
			//GetNode<HSlider>(BRIGHTNESS).ValueChanged += SetBrightness;
			GetNode<OptionButton>(FPS_CAP).ItemSelected += OnFPSCapSelected;
		}

		// Static Setters
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

		private void SetBrightness(double value) {
			var worldEnv = GetNode<WorldEnvironment>(WORLD_ENVIRONMENT);
			float brightness = Mathf.Clamp((float)value, 0.0f, 0.5f);

			if(worldEnv?.Environment != null) {
				worldEnv.Environment.AdjustmentEnable = true;
				worldEnv.Environment.AdjustmentExposure = Mathf.Lerp(0.5f, 2.0f, brightness);
			}
			
			GD.Print($"Brightness set to {brightness}, exposure set to {worldEnv.Environment.AdjustmentExposure}");
		}

		private static void SetFPS(FPS fps) {
			GD.Print($"Setting FPS cap to: {fps}");
			Engine.MaxFps = fps.Value;
		}

		// Callbacks
		private void OnResolutionSelected(long index) => SetResolution(RESOLUTION_OPTIONS[(int)index]);
		private void OnFPSCapSelected(long index) => SetFPS(FPS_OPTIONS[(int)index]);

		// ISaveable implementation
		public DisplaySettings Serialize() {
			Resolution selectedResolution = RESOLUTION_OPTIONS[GetNode<OptionButton>(RESOLUTION).Selected];
			FPS selectedFPS = FPS_OPTIONS[GetNode<OptionButton>(FPS_CAP).Selected];

			return new DisplaySettings {
				Resolution = selectedResolution,
				IsFullscreen = GetNode<CheckBox>(FULLSCREEN).ButtonPressed,
				IsVSyncEnabled = GetNode<CheckBox>(VSYNC).ButtonPressed,
				Brightness = (float)GetNode<HSlider>(BRIGHTNESS).Value,
				FPSCap = selectedFPS.Value
			};
		}

		public void Deserialize(in DisplaySettings data) {
			SetResolution(data.Resolution);
			GetNode<OptionButton>(RESOLUTION).Select(data.Resolution);

			SetFullscreen(data.IsFullscreen);
			GetNode<CheckBox>(FULLSCREEN).ButtonPressed = data.IsFullscreen;

			SetVSync(data.IsVSyncEnabled);
			GetNode<CheckBox>(VSYNC).ButtonPressed = data.IsVSyncEnabled;

			//SetBrightness(data.Brightness);
			//GetNode<HSlider>(BRIGHTNESS).Value = data.Brightness;

			FPS fps = new FPS { Value = data.FPSCap };
			SetFPS(fps);
			GetNode<OptionButton>(FPS_CAP).Select(fps);
		}
	}
}

namespace SaveSystem {
	public readonly record struct DisplaySettings : ISaveData {
		public SettingsPanels.Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}
}
