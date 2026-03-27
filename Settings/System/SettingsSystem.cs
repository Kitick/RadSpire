namespace Settings;

using System;
using Services;

public static class SettingSystem {
	private static readonly LogService Log = new(nameof(SettingSystem), enabled: true);

	private const string SaveFile = "settings";

	public static void Save() {
		new SettingsData {
			Display = DisplaySettings.Export(),
			Audio = AudioSettings.Export(),
			General = GeneralSettings.Export(),
			Accessibility = AccessibilitySettings.Export(),
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

		var data = SaveService.Load<SettingsData>(SaveFile);
		DisplaySettings.Import(data.Display);
		AudioSettings.Import(data.Audio);
		GeneralSettings.Import(data.General);
		AccessibilitySettings.Import(data.Accessibility);
		ControllerSettings.Import(data.Controller);
		MouseKeyboardSettings.Import(data.MouseKeyboard);

		Log.Info("Settings loaded");
		return true;
	}

	public static void Apply() {
		DisplaySettings.Apply();
		AudioSettings.Apply();
		GeneralSettings.Apply();
		AccessibilitySettings.Apply();
		ControllerSettings.Apply();
		MouseKeyboardSettings.Apply();
	}

	public static void Reset() {
		DisplaySettings.Reset();
		AudioSettings.Reset();
		GeneralSettings.Reset();
		AccessibilitySettings.Reset();
		ControllerSettings.Reset();
		MouseKeyboardSettings.Reset();
	}
}

public sealed class Setting<T> : ISetting {
	private readonly LogService Log;
	private readonly Func<T> GetActual;
	private readonly Action<T> SetActual;

	public T Default;

	public T Target {
		get;
		set { Log.Info($"Set {value}"); field = value; }
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

	public void Apply() => Actual = Target;
	public void Apply(T value) { Target = value; Apply(); }
	public void Reset() => Target = Default;
}

public interface ISetting {
	void Apply();
	void Reset();
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
	public GeneralData General { get; init; }
	public AccessibilityData Accessibility { get; init; }
	public ControllerData Controller { get; init; }
	public MouseKeyboardData MouseKeyboard { get; init; }
}
