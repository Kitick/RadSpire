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

		Player.AddCamera(CameraRig);

		Item item = GD.Load<Item>("res://Item/ItemDataBase/Food/AppleRed.tres");
		if(item == null) {
			GD.PrintErr("Failed to load test item.");
		} else {
			GD.Print($"Loaded test item: {item.Name}");
			ItemSlot testSlot = new ItemSlot(item, 5);
			if(!testSlot.IsEmpty()){
				GD.Print($"Test ItemSlot has {testSlot.Quantity} x {testSlot.Item.Name}.");
				Player.PlayerInventory.AddItem(testSlot);
				if(Player.PlayerInventory.GetTotalQuantity(item) >= 5){
					GD.Print("Player inventory successfully contains the test item after addition.");
					Item3DIcon item3DIcon = new Item3DIcon();
					item3DIcon.Item = item;
					item3DIcon.SpawnItem3D(new Vector3(0, 5, 0));
					AddChild(item3DIcon);
				} else {
					GD.PrintErr("Player inventory does not contain the test item after addition.");
				}
			} else {
				GD.PrintErr("Test ItemSlot is empty.");
			}
		}

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