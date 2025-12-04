using System;
using Camera;
using Core;
using Godot;
using SaveSystem;

public sealed partial class GameManager : Node {
	private static Player Player = null!;
	private static CameraRig CameraRig = null!;

	[Export] private string SaveFileName = "autosave";

	public static bool ShouldLoad = true;

	public override void _Ready() {
		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		Player = this.AddScene<Player>(Scenes.Player);

		CameraRig.Target = Player;
		Player.KeyInput.Camera = CameraRig;

		Item item1 = GD.Load<Item>("res://Item/ItemDataBase/Food/AppleRed.tres");
		Item3DIcon item3DIcon1 = new Item3DIcon();
		item3DIcon1.Item = item1;
		item3DIcon1.SpawnItem3D(new Vector3(0, 5, 5));
		AddChild(item3DIcon1);

		Item item2 = GD.Load<Item>("res://Item/ItemDataBase/Food/AppleYellow.tres");
		Item3DIcon item3DIcon2 = new Item3DIcon();
		item3DIcon2.Item = item2;
		item3DIcon2.SpawnItem3D(new Vector3(0, 5, 6));
		AddChild(item3DIcon2);

		Item item3 = GD.Load<Item>("res://Item/ItemDataBase/Food/AppleGreen.tres");
		Item3DIcon item3DIcon3 = new Item3DIcon();
		item3DIcon3.Item = item3;
		item3DIcon3.SpawnItem3D(new Vector3(0, 5, 7));
		AddChild(item3DIcon3);

		Item item4 = GD.Load<Item>("res://Item/ItemDataBase/Food/BananaYellow.tres");
		Item3DIcon item3DIcon4 = new Item3DIcon();
		item3DIcon4.Item = item4;
		item3DIcon4.SpawnItem3D(new Vector3(0, 5, 8));
		AddChild(item3DIcon4);

		Item item5 = GD.Load<Item>("res://Item/ItemDataBase/Food/BananaGreen.tres");
		Item3DIcon item3DIcon5 = new Item3DIcon();
		item3DIcon5.Item = item5;
		item3DIcon5.SpawnItem3D(new Vector3(0, 5, 9));
		AddChild(item3DIcon5);

		Item item6 = GD.Load<Item>("res://Item/ItemDataBase/Food/StrawberryGreen.tres");
		Item3DIcon item3DIcon6 = new Item3DIcon();
		item3DIcon6.Item = item6;
		item3DIcon6.SpawnItem3D(new Vector3(0, 5, 10));
		AddChild(item3DIcon6);

		Item item7 = GD.Load<Item>("res://Item/ItemDataBase/Food/StrawberryRed.tres");
		Item3DIcon item3DIcon7 = new Item3DIcon();
		item3DIcon7.Item = item7;
		item3DIcon7.SpawnItem3D(new Vector3(0, 5, 11));
		AddChild(item3DIcon7);

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