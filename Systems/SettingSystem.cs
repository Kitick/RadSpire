namespace Services.Settings {
	using System;
	using Godot;

	public static class DisplaySettings {
		private static readonly LogService Log = new(nameof(DisplaySettings), enabled: true);

		private static WorldEnvironment? WorldEnv;

		public static void SetWorldEnvironment(WorldEnvironment? env) {
			WorldEnv = env;
			if(WorldEnv is not null) { WorldEnv.Environment.AdjustmentEnabled = true; }
		}

		public static Resolution Resolution {
			get => Resolution.FromVector2I(DisplayServer.WindowGetSize());
			set {
				Log.Info($"Setting resolution to: {value}");
				DisplayServer.WindowSetSize(value.ToVector2I());
			}
		}

		public static bool IsFullscreen {
			get => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
			set {
				var mode = value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
				Log.Info($"Setting fullscreen to: {mode}");
				DisplayServer.WindowSetMode(mode);
			}
		}

		public static bool IsVSync {
			get => DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled;
			set {
				var mode = value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
				Log.Info($"Setting VSync to: {mode}");
				DisplayServer.WindowSetVsyncMode(mode);
			}
		}

		public static float Brightness {
			get => WorldEnv?.Environment.AdjustmentBrightness ?? 1f;
			set {
				if(WorldEnv is null) { Log.Warn("WorldEnvironment not set"); return; }
				Log.Info($"Setting brightness to: {value}");
				WorldEnv.Environment.AdjustmentBrightness = value;
			}
		}

		public static int MaxFps {
			get => Engine.MaxFps;
			set {
				Log.Info($"Setting FPS cap to: {value}");
				Engine.MaxFps = value;
			}
		}
	}

	public static class AudioSettings {
		private static readonly LogService Log = new(nameof(AudioSettings), enabled: true);

		public static int MasterVolume {
			get => AudioBus.Master.GetVolume();
			set {
				Log.Info($"Setting master volume to: {value}");
				AudioBus.Master.SetVolume(value);
			}
		}

		public static int MusicVolume {
			get => AudioBus.Music.GetVolume();
			set {
				Log.Info($"Setting music volume to: {value}");
				AudioBus.Music.SetVolume(value);
			}
		}

		public static int SFXVolume {
			get => AudioBus.SFX.GetVolume();
			set {
				Log.Info($"Setting SFX volume to: {value}");
				AudioBus.SFX.SetVolume(value);
			}
		}

		public static bool IsMuted {
			get => AudioBus.Master.IsMuted();
			set {
				Log.Info($"Setting mute all to: {value}");
				foreach(var bus in AudioBusExtensions.GetAllNames()) {
					bus.SetMuted(value);
				}
			}
		}

		public static string OutputDevice {
			get => AudioServer.OutputDevice;
			set {
				Log.Info($"Setting output device to: {value}");
				AudioServer.OutputDevice = value;
			}
		}
	}

	// --- Display types ---

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

	// --- Audio types ---

	public enum AudioBus { Master, Music, SFX }

	public static class AudioBusExtensions {
		public static AudioBus[] GetAllNames() => Enum.GetValues<AudioBus>();
		public static string GetName(this AudioBus bus) => bus.ToString();

		private static int GetIndex(this AudioBus bus) =>
			AudioServer.GetBusIndex(bus.GetName());

		public static int GetVolume(this AudioBus bus) =>
			(int) Math.Round(AudioServer.GetBusVolumeLinear(bus.GetIndex()) * 100f);

		public static void SetVolume(this AudioBus bus, double volume) =>
			bus.SetVolume((int) Math.Round(volume));

		public static void SetVolume(this AudioBus bus, int volume) {
			AudioServer.SetBusVolumeLinear(bus.GetIndex(), volume / 100f);
		}

		public static bool IsMuted(this AudioBus bus) =>
			AudioServer.IsBusMute(bus.GetIndex());

		public static void SetMuted(this AudioBus bus, bool isMuted) {
			AudioServer.SetBusMute(bus.GetIndex(), isMuted);
		}
	}
}