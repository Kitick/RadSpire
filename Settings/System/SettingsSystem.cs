namespace Settings;

using System;
using System.Numerics;
using Root;
using Services;

public static class SettingSystem {
	private static readonly LogService Log = new(nameof(SettingSystem), enabled: true);

	private const string SaveFile = "settings";

	public static void Save() {
		new SettingsData {
			Display = DisplaySettings.Export(),
			Audio = AudioSettings.Export(),
			Controller = ControllerSettings.Export(),
			MouseKeyboard = MouseKeyboardSettings.Export(),
		}.Save(SaveFile);
		Log.Info("Settings saved");
	}

	public static bool Load() {
		if(!SaveService.Exists(SaveFile)) {
			Log.Info("No settings file found, using defaults");
			return false;
		}

		try {
			SettingsData data = SaveService.Load<SettingsData>(SaveFile);
			DisplaySettings.Import(data.Display);
			AudioSettings.Import(data.Audio);
			ControllerSettings.Import(data.Controller);
			MouseKeyboardSettings.Import(data.MouseKeyboard);
		}
		catch(Exception e) {
			Log.Info($"Settings file invalid, resetting to defaults: {e.Message}");
			Save();
			return false;
		}

		Save();
		Log.Info("Settings loaded");
		return true;
	}

	public static void Apply() {
		DisplaySettings.Apply();
		AudioSettings.Apply();
		ControllerSettings.Apply();
		MouseKeyboardSettings.Apply();
	}

	public static void Reset() {
		DisplaySettings.Reset();
		AudioSettings.Reset();
		ControllerSettings.Reset();
		MouseKeyboardSettings.Reset();
	}
}

public class Setting<T> : ISetting {
	private readonly LogService Log;
	private readonly Func<T> GetActual;
	private readonly Action<T> SetActual;

	public T Default;

	public T Target {
		get;
		set { Log.Info($"Set {value}"); field = ProcessTarget(value); }
	}

	public T Actual {
		get => GetActual();
		set { Log.Info($"Set {value}"); SetActual(value); }
	}

	public Setting(string name, Func<T> getActual, Action<T> setActual, T defaultValue) {
		Log = new LogService(name, enabled: true);
		GetActual = getActual;
		SetActual = setActual;
		Target = defaultValue;
		Default = defaultValue;
	}

	protected virtual T ProcessTarget(T value) => value;

	public void Apply() => Actual = Target;
	public void Apply(T value) { Target = value; Apply(); }
	public void Reset() => Target = Default;
}

public sealed class SliderSetting<T> : Setting<T>, ISliderSetting where T : struct, INumber<T> {
	public T Min { get; }
	public T Max { get; }
	public T Step { get; }

	double ISliderSetting.Min => double.CreateTruncating(Min);
	double ISliderSetting.Max => double.CreateTruncating(Max);
	double ISliderSetting.Step => double.CreateTruncating(Step);

	public SliderSetting(string name, Func<T> getActual, Action<T> setActual, T defaultValue, T min, T max, T step)
		: base(name, getActual, setActual, defaultValue) {
		Min = min;
		Max = max;
		Step = step;
	}

	protected override T ProcessTarget(T value) => T.Clamp(value, Min, Max).Round(Step);
}

public sealed class OptionSetting<T> : Setting<T>, IOptionSetting where T : notnull {
	public T[] Options { get; }

	object[] IOptionSetting.Options => Array.ConvertAll(Options, o => (object) o);

	public OptionSetting(string name, Func<T> getActual, Action<T> setActual, T[] options, T? defaultValue = default)
		: base(name, getActual, setActual, defaultValue ?? options[0]) {
		Options = options;
	}

	protected override T ProcessTarget(T value) {
		foreach(T option in Options) {
			if(option.Equals(value)) return value;
		}
		return Default;
	}
}

public interface ISetting {
	void Apply();
	void Reset();
}

public interface ISliderSetting {
	double Min { get; }
	double Max { get; }
	double Step { get; }
}

public interface IOptionSetting {
	object[] Options { get; }
}

public static class SliderExtensions {
	public static void ApplyBounds(this Godot.HSlider slider, ISliderSetting setting) {
		slider.MinValue = setting.Min;
		slider.MaxValue = setting.Max;
		slider.Step = setting.Step;
	}
}


public static class SettingExtensions {
	public static void Apply(this ISetting[] settings) {
		foreach(var setting in settings) { setting.Apply(); }
	}

	public static void Reset(this ISetting[] settings) {
		foreach(var setting in settings) { setting.Reset(); }
	}
}

public readonly record struct SettingsData : ISaveData {
	public DisplayData Display { get; init; }
	public AudioData Audio { get; init; }
	public ControllerData Controller { get; init; }
	public MouseKeyboardData MouseKeyboard { get; init; }
}
