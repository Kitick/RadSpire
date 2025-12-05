using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class GameManager : Node {
	public static GameManager Instance { get; private set; } = null!;

	private static readonly Logger Log = new(nameof(GameManager), enabled: true);

	public bool InGame => GetTree().CurrentScene?.SceneFilePath == Scenes.GameScene;

	public Player? LocalPlayer;
	public CameraRig? CameraRig;
	public Enemy? Enemy;

	private static readonly Vector3 SpawnLocation = new Vector3(0, 5, 0);

	private readonly KeyInput KeyInput = new();

	private float SpawnTimer = 5.0f;
	private int EnemyCount;
	private PackedScene EnemyScene = null!;

	public override void _Ready() {
		EnemyScene = GD.Load<PackedScene>("res://Character/Enemy/Enemy.tscn");
		Instance = this;

		InitializeNetwork();
	}

	public override void _ExitTree() {
		CleanupNetwork();
	}

	public override void _PhysicsProcess(double delta) {
		if(!InGame) { return; }

		float dt = (float) delta;

		KeyInput.Update(CameraRig!);
		LocalPlayer!.Update(dt, KeyInput);
		
		UpdateTimer();
	}

	private void SpawnLocalPlayer() {
		LocalPlayer = this.AddScene<Player>(Scenes.Player);

		LocalPlayer.Name = $"Player_{LocalPeerId}";
		LocalPlayer.GlobalPosition = SpawnLocation;

		CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		CameraRig.Target = LocalPlayer;

	}

	private void UpdateTimer()
	{
		SpawnTimer -= 0.015f;

		if(SpawnTimer <= 0.0f && EnemyCount < 5) {
			GD.Print("Spawned");
			SpawnTimer = (float) GD.RandRange(1f, 6f);
			Enemy = EnemyScene.Instantiate<Enemy>();
			AddChild(Enemy);
			Enemy.GlobalPosition = GetRandomEnemySpawn();
			EnemyCount += 1;
		}
	}
	
	private Vector3 GetRandomEnemySpawn()
	{
		var pos = LocalPlayer.GlobalPosition;
		return pos + new Vector3(
			(float)GD.RandRange(-10f, 10f),
			0.25f,
			(float)GD.RandRange(-10f, 10f)
		);
		
	}

	public void DecrementEnemyCount() {
		EnemyCount -= 1;
	}

	public bool Save(string fileName) {
		if(!InGame) {
			Log.Error("Cannot save game when not in a game");
			return false;
		}

		if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(Enemy) || !IsInstanceValid(CameraRig)) {
			Log.Error("Cannot save game: game objects are not valid");
			return false;
		}

		var data = new GameState {
			Player = LocalPlayer!.Serialize(),
			Enemy = Enemy!.Serialize(),
			CameraRig = CameraRig!.Serialize(),
		};

		SaveService.Save(fileName, data);
		return true;
	}

	public bool Load(string fileName) {
		if(!InGame) {
			Log.Error("Cannot load game when not in a game");
			return false;
		}

		if(!SaveService.Exists(fileName)) {
			Log.Error($"Save file '{fileName}' does not exist");
			return false;
		}

		var data = SaveService.Load<GameState>(fileName);

		LocalPlayer!.Deserialize(data.Player);
		Enemy!.Deserialize(data.Enemy);
		CameraRig!.Deserialize(data.CameraRig);

		return true;
	}

	public void StartGame() {
		CleanupGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.GameScene);
		SpawnLocalPlayer();
		SpawnTestItems();
	}

	public void ReturnToMainMenu() {
		CleanupGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}

	public void ExitApplication() {
		CleanupGame();
		GetTree().Quit();
	}

	private void CleanupGame() {
		LocalPlayer?.QueueFree();
		LocalPlayer = null;

		Enemy?.QueueFree();
		Enemy = null;

		CameraRig?.QueueFree();
		CameraRig = null;
	}

	private void SpawnTestItem(string path, Vector3 position) {
		Item item = GD.Load<Item>(path);
		Item3DIcon item3DIcon = new Item3DIcon();
		item3DIcon.Item = item;
		item3DIcon.Name = item.Name + "3DIcon";
		AddChild(item3DIcon);
		item3DIcon.SpawnItem3D(position);
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
}

namespace SaveSystem {
	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
		public EnemyData Enemy { get; init; }
	}
}