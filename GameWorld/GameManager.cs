using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class GameManager : Node {
	public static GameManager Instance { get; private set; } = null!;

	public static readonly bool Debug = true;

	public bool InGame => GetTree().CurrentScene.SceneFilePath == Scenes.GameScene;

	public Player LocalPlayer { get; private set; } = null!;

	public CameraRig CameraRig { get; private set; } = null!;

	private readonly KeyInput KeyInput = new();

	public override void _Ready() {
		Instance = this;

		InitializeNetwork();
		GetComponents();
	}

	public override void _ExitTree() {
		CleanupNetwork();
	}

	public override void _PhysicsProcess(double delta) {
		if(!InGame) { return; }

		float dt = (float) delta;

		KeyInput.Update(CameraRig);
		LocalPlayer.Update(dt, KeyInput);
		// NetworkSync handles broadcasting automatically via Movement.OnStateChanged
	}

	private void GetComponents() {
		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		LocalPlayer = this.AddScene<Player>(Scenes.Player);
		LocalPlayer.Name = $"Player_{LocalPeerId}";

		CameraRig.Target = LocalPlayer;
	}

	private static void Log(string message) {
		if(Debug) { GD.Print($"[GameManager] {message}"); }
	}

	private void SpawnTestItem(string path, Vector3 position) {
		Item item = GD.Load<Item>(path);
		Item3DIcon item3DIcon = new Item3DIcon();
		item3DIcon.Item = item;
		item3DIcon.Name = item.Name + "3DIcon";
		item3DIcon.SpawnItem3D(position);
		AddChild(item3DIcon);
	}

	private void SpawnTestItems() {
		SpawnTestItem(Items.AppleRed, new Vector3(0, 5, 5));
		SpawnTestItem(Items.AppleYellow, new Vector3(0, 5, 6));
		SpawnTestItem(Items.AppleGreen, new Vector3(0, 5, 7));
		SpawnTestItem(Items.BananaYellow, new Vector3(0, 5, 8));
		SpawnTestItem(Items.BananaGreen, new Vector3(0, 5, 9));
		SpawnTestItem(Items.StrawberryGreen, new Vector3(0, 5, 10));
		SpawnTestItem(Items.StrawberryRed, new Vector3(0, 5, 11));
	}

	public bool Save(string fileName) {
		if(!InGame) {
			GD.PrintErr("Cannot save game when not in a game");
			return false;
		}

		var data = new GameState {
			Player = LocalPlayer.Serialize(),
			CameraRig = CameraRig.Serialize(),
		};

		SaveService.Save(fileName, data);
		return true;
	}

	public bool Load(string fileName) {
		if(!InGame) {
			GD.PrintErr("Cannot load game when not in a game");
			return false;
		}

		if(!SaveService.Exists(fileName)) {
			GD.PrintErr($"Save file '{fileName}' does not exist");
			return false;
		}

		var data = SaveService.Load<GameState>(fileName);

		LocalPlayer.Deserialize(data.Player);
		CameraRig.Deserialize(data.CameraRig);

		return true;
	}

	public void StartGame() {
		GetTree().ChangeSceneToFile(Scenes.GameScene);
		SpawnTestItems();
	}

	public void QuitGame() {
		GetTree().Quit();
	}
}

namespace SaveSystem {
	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}