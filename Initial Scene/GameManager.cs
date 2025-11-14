using System;
using Godot;
using SaveSystem;

public partial class GameManager : Node {
	public static ISaveable<PlayerData>? Player;
	public static ISaveable<CameraPivotData>? CameraPivot;
	public static ISaveable<CameraRigData>? CameraRig;

	[Export] private string SaveFileName = "autosave";
	public static bool ShouldLoad = true;

	public override void _Ready() {
		if(ShouldLoad) { CallDeferred(nameof(Load), SaveFileName); }
	}

	public static void Save(string fileName) {
		var data = new GameState {
			Player = Player?.Serialize() ?? default,
			CameraPivot = CameraPivot?.Serialize() ?? default,
			CameraRig = CameraRig?.Serialize() ?? default
		};

		SaveService.Save(fileName, data);
	}

	public static bool Load(string fileName) {
		if(SaveService.Exists(fileName)) {
			var data = SaveService.Load<GameState>(fileName);

			Player?.Deserialize(data.Player);
			CameraPivot?.Deserialize(data.CameraPivot);
			CameraRig?.Deserialize(data.CameraRig);

			return true;
		}
		return false;
	}
}

namespace SaveSystem {
	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraPivotData CameraPivot { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}