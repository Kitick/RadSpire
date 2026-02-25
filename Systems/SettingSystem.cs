namespace Services.Settings {
	using System;
	using Godot;

	public static class SettingSystem {
		private static readonly LogService Log = new(nameof(SettingSystem), enabled: true);

		private const string SaveFile = "settings";

		public static void Save() {
			new SettingsData {
				Display = DisplaySettings.Export(),
				Audio = AudioSettings.Export(),
			}.Save(SaveFile);
			Log.Info("Settings saved");
		}

		public static bool Load() {
			if(!SaveService.Exists(SaveFile)) {
				Log.Info("No settings file found, using defaults");
				return false;
			}

			var data = SaveService.Load<SettingsData>(SaveFile);
			DisplaySettings.Import(data.Display);
			AudioSettings.Import(data.Audio);

			Log.Info("Settings loaded");
			return true;
		}
	}

	public sealed class Setting<T> {
		private readonly LogService Log;
		private readonly Func<T> GetActual;
		private readonly Action<T> SetActual;

		public T Default;
		public T Target;

		public T Actual {
			get => GetActual();
			set { Log.Info($"Applying: {value}"); SetActual(value); }
		}

		public Setting(string name, Func<T> getActual, Action<T> setActual, T defaultValue) {
			Log = new LogService(name, enabled: true);
			GetActual = getActual;
			SetActual = setActual;
			Target = defaultValue;
			Default = defaultValue;
		}

		public void Apply() => Actual = Target;
		public void Reset() => Target = Default;
	}

	public static class DisplaySettings {
		private static WorldEnvironment? WorldEnv;

		public static void SetWorldEnvironment(WorldEnvironment? env) {
			WorldEnv = env;
			WorldEnv?.Environment.AdjustmentEnabled = true;
		}

		public static readonly Setting<Resolution> Resolution = new(
			name: nameof(Resolution),
			getActual: () => { var s = DisplayServer.WindowGetSize(); return new Resolution { Width = s.X, Height = s.Y }; },
			setActual: v => DisplayServer.WindowSetSize(v.ToVector2I()),
			defaultValue: new Resolution { Width = 1280, Height = 720 }
		);

		public static readonly Setting<bool> IsFullscreen = new(
			name: nameof(IsFullscreen),
			getActual: () => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen,
			setActual: v => DisplayServer.WindowSetMode(v ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed),
			defaultValue: false
		);

		public static readonly Setting<bool> IsVSync = new(
			name: nameof(IsVSync),
			getActual: () => DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled,
			setActual: v => DisplayServer.WindowSetVsyncMode(v ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled),
			defaultValue: false
		);

		public static readonly Setting<float> Brightness = new(
			name: nameof(Brightness),
			getActual: () => WorldEnv?.Environment.AdjustmentBrightness ?? 1f,
			setActual: v => {
				if(WorldEnv is null) { GD.PrintErr("Brightness: WorldEnvironment not set"); return; }
				WorldEnv.Environment.AdjustmentBrightness = v;
			},
			defaultValue: 1f
		);

		public static readonly Setting<int> MaxFps = new(
			name: nameof(MaxFps),
			getActual: () => Engine.MaxFps,
			setActual: v => Engine.MaxFps = v,
			defaultValue: 0
		);

		public static void Apply() {
			Resolution.Apply();
			IsFullscreen.Apply();
			IsVSync.Apply();
			Brightness.Apply();
			MaxFps.Apply();
		}

		public static void Reset() {
			Resolution.Reset();
			IsFullscreen.Reset();
			IsVSync.Reset();
			Brightness.Reset();
			MaxFps.Reset();
		}

		public static DisplayData Export() => new DisplayData {
			Resolution = Resolution.Target,
			IsFullscreen = IsFullscreen.Target,
			IsVSyncEnabled = IsVSync.Target,
			Brightness = Brightness.Target,
			FPSCap = MaxFps.Target,
		};

		public static void Import(DisplayData data) {
			Resolution.Target = data.Resolution;
			IsFullscreen.Target = data.IsFullscreen;
			IsVSync.Target = data.IsVSyncEnabled;
			Brightness.Target = data.Brightness;
			MaxFps.Target = data.FPSCap;
		}
	}

	public static class AudioSettings {
		public static readonly Setting<int> MasterVolume = new(
			name: nameof(MasterVolume),
			getActual: () => AudioBus.Master.GetVolume(),
			setActual: v => AudioBus.Master.SetVolume(v),
			defaultValue: 100
		);

		public static readonly Setting<int> MusicVolume = new(
			name: nameof(MusicVolume),
			getActual: () => AudioBus.Music.GetVolume(),
			setActual: v => AudioBus.Music.SetVolume(v),
			defaultValue: 100
		);

		public static readonly Setting<int> SFXVolume = new(
			name: nameof(SFXVolume),
			getActual: () => AudioBus.SFX.GetVolume(),
			setActual: v => AudioBus.SFX.SetVolume(v),
			defaultValue: 100
		);

		public static readonly Setting<bool> IsMuted = new(
			name: nameof(IsMuted),
			getActual: () => AudioBus.Master.IsMuted(),
			setActual: v => { foreach(var bus in AudioBusExtensions.GetAllNames()) { bus.SetMuted(v); } },
			defaultValue: false
		);

		public static readonly Setting<string> OutputDevice = new(
			name: nameof(OutputDevice),
			getActual: () => AudioServer.OutputDevice,
			setActual: v => AudioServer.OutputDevice = v,
			defaultValue: "Default"
		);

		public static void Apply() {
			MasterVolume.Apply();
			MusicVolume.Apply();
			SFXVolume.Apply();
			IsMuted.Apply();
			OutputDevice.Apply();
		}

		public static void Reset() {
			MasterVolume.Reset();
			MusicVolume.Reset();
			SFXVolume.Reset();
			IsMuted.Reset();
			OutputDevice.Reset();
		}

		public static AudioData Export() => new AudioData {
			MasterVolume = MasterVolume.Target,
			MusicVolume = MusicVolume.Target,
			SFXVolume = SFXVolume.Target,
			IsMuted = IsMuted.Target,
			OutputDevice = OutputDevice.Target,
		};

		public static void Import(AudioData data) {
			MasterVolume.Target = data.MasterVolume;
			MusicVolume.Target = data.MusicVolume;
			SFXVolume.Target = data.SFXVolume;
			IsMuted.Target = data.IsMuted;
			OutputDevice.Target = data.OutputDevice ?? OutputDevice.Default;
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

	public readonly record struct DisplayData : ISaveData {
		public Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}

	public readonly record struct AudioData : ISaveData {
		public int MasterVolume { get; init; }
		public int MusicVolume { get; init; }
		public int SFXVolume { get; init; }
		public bool IsMuted { get; init; }
		public string OutputDevice { get; init; }
	}

	public readonly record struct SettingsData : ISaveData {
		public DisplayData Display { get; init; }
		public AudioData Audio { get; init; }
	}
}