namespace Settings;

using Godot;
using Services;

public static class DisplaySettings {
	private static WorldEnvironment? WorldEnv;

	public static void SetWorldEnvironment(WorldEnvironment? env) {
		if(env != null && !GodotObject.IsInstanceValid(env)) {
			env = null;
		}
		WorldEnv = env;
		if(WorldEnv is not null && GodotObject.IsInstanceValid(WorldEnv) && WorldEnv.Environment != null) {
			WorldEnv.Environment.AdjustmentEnabled = true;
			Brightness.Apply();
		}
	}

	public static readonly OptionSetting<Resolution> Resolution = new(
		name: nameof(Resolution),
		getActual: () => { Vector2I s = DisplayServer.WindowGetSize(); return new Resolution { Width = s.X, Height = s.Y }; },
		setActual: v => DisplayServer.WindowSetSize(v.ToVector2I()),
		options: [
			new Resolution { Width = 1280, Height = 720 },
			new Resolution { Width = 1600, Height = 900 },
			new Resolution { Width = 1920, Height = 1080 },
			new Resolution { Width = 2560, Height = 1440 },
			new Resolution { Width = 3840, Height = 2160 },
		]
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
		defaultValue: true
	);

	public static readonly SliderSetting<float> Brightness = new(
		name: nameof(Brightness),
		getActual: () => WorldEnv?.Environment.AdjustmentBrightness ?? 1f,
		setActual: v => {
			if(WorldEnv == null || WorldEnv.Environment == null) {
				GD.PushWarning("Brightness: WorldEnvironment not set");
				return;
			}
			WorldEnv.Environment.AdjustmentBrightness = v;
		},
		defaultValue: 1f,
		min: 0.2f,
		max: 2f,
		step: 0.1f
	);

	public static readonly OptionSetting<Framerate> MaxFps = new(
		name: nameof(MaxFps),
		getActual: () => new Framerate { Value = Engine.MaxFps },
		setActual: v => Engine.MaxFps = v.Value,
		options: [
			new Framerate { Value = 0 },
			new Framerate { Value = 30 },
			new Framerate { Value = 60 },
			new Framerate { Value = 120 },
			new Framerate { Value = 144 },
			new Framerate { Value = 165 },
		]
	);

	private static readonly ISetting[] All = [Resolution, IsFullscreen, IsVSync, Brightness, MaxFps];

	public static void Apply() => All.Apply();
	public static void Reset() => All.Reset();

	public static DisplayData Export() => new() {
		Resolution = Resolution.Target,
		IsFullscreen = IsFullscreen.Target,
		IsVSyncEnabled = IsVSync.Target,
		Brightness = Brightness.Target,
		FPSCap = MaxFps.Target,
	};

	public static void Import(DisplayData data) {
		Resolution.Target = data.Resolution ?? Resolution.Default;
		IsFullscreen.Target = data.IsFullscreen ?? IsFullscreen.Default;
		IsVSync.Target = data.IsVSyncEnabled ?? IsVSync.Default;
		Brightness.Target = data.Brightness ?? Brightness.Default;
		MaxFps.Target = data.FPSCap ?? MaxFps.Default;
	}

}

public readonly record struct Resolution {
	public int Width { get; init; }
	public int Height { get; init; }

	public readonly Vector2I ToVector2I() => new(Width, Height);
	public static Resolution FromVector2I(Vector2I size) => new() { Width = size.X, Height = size.Y };

	public override string ToString() => $"{Height}p";
}

public readonly record struct Framerate {
	public int Value { get; init; }

	public override string ToString() => Value == 0 ? "Unlimited" : $"{Value} FPS";
}

public readonly record struct DisplayData : ISaveData {
	public Resolution? Resolution { get; init; }
	public bool? IsFullscreen { get; init; }
	public bool? IsVSyncEnabled { get; init; }
	public float? Brightness { get; init; }
	public Framerate? FPSCap { get; init; }
}
