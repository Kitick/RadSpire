namespace Services.Settings {
	using System;

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

		public static void Apply() {
			DisplaySettings.Apply();
			AudioSettings.Apply();
		}

		public static void Reset() {
			DisplaySettings.Reset();
			AudioSettings.Reset();
		}
	}

	public sealed class Setting<T> {
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

	public readonly record struct SettingsData : ISaveData {
		public DisplayData Display { get; init; }
		public AudioData Audio { get; init; }
	}
}