using System;
using Components;
using Camera;
using Core;
using Godot;
using SaveSystem;

public sealed partial class GameManager : Node {
	private static Player Player = null!;
	private static CameraRig CameraRig = null!;

	private static KeyInput KeyInput => new KeyInput();

	[Export] private string SaveFileName = "autosave";

	public static bool ShouldLoad = true;

	private void SpawnTestItem(string path, Vector3 position) {
		Item item = GD.Load<Item>(path);
		Item3DIcon item3DIcon = new Item3DIcon();
		item3DIcon.Item = item;
		item3DIcon.SpawnItem3D(position);
		AddChild(item3DIcon);
	}

	public override void _Ready() {
		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		Player = this.AddScene<Player>(Scenes.Player);

		CameraRig.Target = Player;
		KeyInput.Camera = CameraRig;

		SpawnTestItem(Items.AppleRed, new Vector3(0, 5, 5));
		SpawnTestItem(Items.AppleYellow, new Vector3(0, 5, 6));
		SpawnTestItem(Items.AppleGreen, new Vector3(0, 5, 7));
		SpawnTestItem(Items.BananaYellow, new Vector3(0, 5, 8));
		SpawnTestItem(Items.BananaGreen, new Vector3(0, 5, 9));
		SpawnTestItem(Items.StrawberryGreen, new Vector3(0, 5, 10));
		SpawnTestItem(Items.StrawberryRed, new Vector3(0, 5, 11));

		if(ShouldLoad) { Load(SaveFileName); }
	}

	public override void _PhysicsProcess(double delta) {
		Player.Update(delta, KeyInput);
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