using System;
using Core;
using Godot;
using SaveSystem;

public partial class GameManager : Node {
	private static Player Player = null!;
	private static CameraRig CameraRig = null!;

	[Export] private string SaveFileName = "autosave";

	public static bool ShouldLoad = true;

	public override void _Ready() {
		Player = this.AddScene<Player>(Scenes.Player);
		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);

		if(ShouldLoad) { Load(SaveFileName); }

		FollowPlayer();
	}

	public static void FollowPlayer() {
		if(Player != null && CameraRig != null) {
			CameraRig.SetTarget(Player);
		}
		else {
			GD.PrintErr("Failed to set camera target - Player or CameraRig is null");
		}
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