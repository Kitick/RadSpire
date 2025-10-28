using Godot;

namespace SaveSystem {
	interface ISaveData;

	static class Example {
		public static void ExampleSave() {
			var playerData = new PlayerData {
				Position = new Vector3(1, 2, 3),
				Rotation = new Vector3(0, 90, 0),
				Health = 85f
			};

			var settings = new GameSettings {
				ResolutionWidth = 2560,
				ResolutionHeight = 1440,
				Fullscreen = FullscreenMode.Fullscreen,
				MusicVolume = 0.7f,
				SFXVolume = 0.6f
			};

			SaveService.Save("player", playerData);
			SaveService.Save("settings", settings);

			GD.Print("Saved:");
			GD.Print(playerData);
			GD.Print(settings);
		}

		public static void ExampleLoad() {
			var saves = SaveService.GetSaves();

			GD.Print("\nAvailable Saves:");
			foreach(var save in saves) {
				GD.Print(save);
			}

			var loadedPlayerData = SaveService.Load<PlayerData>("player");
			var loadedSettings = SaveService.Load<GameSettings>("settings");

			GD.Print("\nLoaded:");
			GD.Print(loadedPlayerData);
			GD.Print(loadedSettings);
		}

		public static void ExampleDelete() {
			SaveService.Delete("player");
			SaveService.Delete("settings");

			GD.Print("\nDeleted 'player' and 'settings' saves.");
		}
	}

	record PlayerData : ISaveData {
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }

		public float Health { get; init; } = 100f;
	}

	enum FullscreenMode { Windowed, Fullscreen, Borderless }

	record GameSettings : ISaveData {
		public int ResolutionWidth { get; init; } = 1920;
		public int ResolutionHeight { get; init; } = 1080;
		public FullscreenMode Fullscreen { get; init; } = FullscreenMode.Borderless;

		public float MusicVolume { get; init; } = 0.5f;
		public float SFXVolume { get; init; } = 0.5f;
	}
}