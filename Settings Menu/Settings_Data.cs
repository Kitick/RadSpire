using SettingsPanels;

namespace SaveSystem {
	public readonly record struct SettingsData : ISaveData {
		public DisplaySettings DisplaySettings { get; init; }
		public SoundSettings SoundSettings { get; init; }
	}

	public readonly record struct DisplaySettings : ISaveData {
		public Resolution Resolution { get; init; }
		public bool IsFullscreen { get; init; }
		public bool IsVSyncEnabled { get; init; }
		public float Brightness { get; init; }
		public int FPSCap { get; init; }
	}

	public readonly record struct SoundSettings : ISaveData {
		public float MasterVolume { get; init; }
		public float MusicVolume { get; init; }
		public float SFXVolume { get; init; }
		public bool IsMuted { get; init; }
		public string OutputDevice { get; init; }
	}
}
