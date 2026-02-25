namespace Services.Settings {
	using System;
	using Godot;

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

	public readonly record struct AudioData : ISaveData {
		public int MasterVolume { get; init; }
		public int MusicVolume { get; init; }
		public int SFXVolume { get; init; }
		public bool IsMuted { get; init; }
		public string OutputDevice { get; init; }
	}
}
