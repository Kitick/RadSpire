using System.Diagnostics;
using Core;
using Godot;
using SaveSystem;

namespace Settings {
	public readonly record struct Resolution {
		public int Width { get; init; }
		public int Height { get; init; }

		public readonly Vector2I ToVector2I() => new(Width, Height);
		public static Resolution FromVector2I(Vector2I size) => new Resolution { Width = size.X, Height = size.Y };

		public override string ToString() => $"{Height}p";
	}

	public readonly record struct FPS {
		public int Value { get; init; }

		public override string ToString() => Value == 0 ? "Unlimited" : $"{Value} FPS";
	}

	public sealed partial class DisplayPanel : VBoxContainer, ISaveable<DisplaySettings> {
		public static bool Debug = false;

		// Node Paths
		private const string RESOLUTION = "Resolution/OptionButton";
		private const string FULLSCREEN = "Fullscreen/CheckBox";
		private const string VSYNC = "VSync/CheckBox";
		private const string BRIGHTNESS = "Brightness/HSlider";
		private const string FPS_CAP = "FPS_Cap/OptionButton";
		private const string WORLD_ENVIRONMENT = "../WorldEnvironment";

		private WorldEnvironment WorldEnv = null!;

		// Options
		private static readonly Resolution[] RESOLUTION_OPTIONS = [
			new Resolution { Width = 1280, Height = 720 },
			new Resolution { Width = 1600, Height = 900 },
			new Resolution { Width = 1920, Height = 1080 },
			new Resolution { Width = 2560, Height = 1440 },
			new Resolution { Width = 3840, Height = 2160 },
		];

		private static readonly FPS[] FPS_OPTIONS = [
			new FPS { Value = 0 }, //Unlimited
			new FPS { Value = 30 },
			new FPS { Value = 60 },
			new FPS { Value = 120 },
			new FPS { Value = 144 },
			new FPS { Value = 165 },
		];

		// Main
		public override void _Ready() {
			GetNode<OptionButton>(RESOLUTION).Populate(RESOLUTION_OPTIONS);
			GetNode<OptionButton>(FPS_CAP).Populate(FPS_OPTIONS);

			GetComponenets();
			SetCallbacks();
		}

		private void GetComponenets() {
			WorldEnv = GetNode<WorldEnvironment>(WORLD_ENVIRONMENT);
			WorldEnv.Environment.AdjustmentEnabled = true;
		}

		// Callbacks
		private void SetCallbacks() {
			GetNode<OptionButton>(RESOLUTION).ItemSelected += (index) => Resolution = RESOLUTION_OPTIONS[(int) index];
			GetNode<CheckBox>(FULLSCREEN).Toggled += (value) => Fullscreen = value;
			GetNode<CheckBox>(VSYNC).Toggled += (value) => VSync = value;
			GetNode<HSlider>(BRIGHTNESS).ValueChanged += (value) => Brightness = (float) value;
			GetNode<OptionButton>(FPS_CAP).ItemSelected += (index) => FPS = FPS_OPTIONS[(int) index];
		}

		// Resolution
		public static Resolution Resolution {
			get => Resolution.FromVector2I(DisplayServer.WindowGetSize());
			set {
				if(Debug) { GD.Print($"Setting resolution to: {value}"); }
				DisplayServer.WindowSetSize(value.ToVector2I());
			}
		}

		// Fullscreen
		public static bool Fullscreen {
			get => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
			set {
				var mode = value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
				if(Debug) { GD.Print($"Setting fullscreen to: {mode}"); }
				DisplayServer.WindowSetMode(value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
			}
		}

		// VSYNC
		public static bool VSync {
			get => DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
			set {
				var mode = value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
				if(Debug) { GD.Print($"Setting VSync to: {mode}"); }
				DisplayServer.WindowSetVsyncMode(mode);
			}
		}

		public float Brightness {
			get => WorldEnv.Environment.AdjustmentBrightness;
			set {
				if(Debug) { GD.Print($"Setting brightness to: {value}"); }
				WorldEnv.Environment.AdjustmentBrightness = Mathf.Clamp(value, 0, 2);
			}
		}

		public static FPS FPS {
			get => new FPS { Value = Engine.MaxFps };
			set {
				if(Debug) { GD.Print($"Setting FPS cap to: {value}"); }
				Engine.MaxFps = value.Value;
			}
		}

		// ISaveable implementation
		public DisplaySettings Serialize() {
			Resolution selectedResolution = RESOLUTION_OPTIONS[GetNode<OptionButton>(RESOLUTION).Selected];
			FPS selectedFPS = FPS_OPTIONS[GetNode<OptionButton>(FPS_CAP).Selected];

			return new DisplaySettings {
				Resolution = selectedResolution,
				IsFullscreen = GetNode<CheckBox>(FULLSCREEN).ButtonPressed,
				IsVSyncEnabled = GetNode<CheckBox>(VSYNC).ButtonPressed,
				Brightness = (float) GetNode<HSlider>(BRIGHTNESS).Value,
				FPSCap = selectedFPS.Value
			};
		}

		public void Deserialize(in DisplaySettings data) {
			Resolution = data.Resolution;
			GetNode<OptionButton>(RESOLUTION).Select(data.Resolution);

			Fullscreen = data.IsFullscreen;
			GetNode<CheckBox>(FULLSCREEN).ButtonPressed = data.IsFullscreen;

			VSync = data.IsVSyncEnabled;
			GetNode<CheckBox>(VSYNC).ButtonPressed = data.IsVSyncEnabled;

			Brightness = data.Brightness;
			GetNode<HSlider>(BRIGHTNESS).Value = data.Brightness;

			FPS = new FPS { Value = data.FPSCap };
			GetNode<OptionButton>(FPS_CAP).Select(FPS);
		}
	}
}

namespace SaveSystem {
	public readonly record struct DisplaySettings : ISaveData {
		public Settings.Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}
}