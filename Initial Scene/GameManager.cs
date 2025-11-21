using System;
using Camera;
using Core;
using Godot;
using SaveSystem;

public partial class GameManager : Node {
	private static Player Player = null!;
	private static CameraRig CameraRig = null!;

	[Export] private string SaveFileName = "autosave";

	public static bool ShouldLoad = true;

	public override void _Ready() {
		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		Player = this.AddScene<Player>(Scenes.Player);

		Player.AddCamera(CameraRig);

		if(ShouldLoad) { Load(SaveFileName); }
	}

	public static void Save(string fileName) {
		var data = new GameState {
			Player = Player.Serialize(),
			CameraRig = CameraRig.Serialize(),
		};

		SaveService.Save(fileName, data);
	}

	public static bool Load(string fileName) {
		if(SaveService.Exists(fileName)) {
			var data = SaveService.Load<GameState>(fileName);

			Player.Deserialize(data.Player);
			CameraRig.Deserialize(data.CameraRig);

			return true;
		}
		return false;
	}
}

namespace SaveSystem {
	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}