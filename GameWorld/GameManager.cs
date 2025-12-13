//This file was developed entirely by the RadSpire Development Team.

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

	private const int SpawnHeight = 5;
	private const int SpawnRadius = 50;

	private static readonly Vector3 PlayerSpawnLocation = new Vector3(0, SpawnHeight, 0);

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
		if(!InGame || !IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig)) { return; }

		float dt = (float) delta;

		KeyInput.Update(CameraRig);
		LocalPlayer.Update(dt, KeyInput);

		UpdateTimer();
	}

	public Player SpawnLocalPlayer() {
		LocalPlayer = this.AddScene<Player>(Scenes.Player);

		LocalPlayer.Name = $"Player_{LocalPeerId}";
		LocalPlayer.GlobalPosition = PlayerSpawnLocation;

		if(!IsInstanceValid(CameraRig)) {
			CameraRig = this.AddScene<CameraRig>(Scenes.Camera);
		}

		CameraRig.Target = LocalPlayer;

		return LocalPlayer;
	}

	public Player RespawnPlayer() {
		if(!IsInstanceValid(LocalPlayer)) {
			Log.Warn("RespawnPlayer called but no player exists, spawning new player");
			return SpawnLocalPlayer();
		}

		var inventoryData = LocalPlayer.Inventory.Serialize();
		var hotbarData = LocalPlayer.Hotbar.Serialize();

		LocalPlayer.QueueFree();
		LocalPlayer = null;

		SpawnLocalPlayer();

		LocalPlayer!.Inventory.Deserialize(inventoryData);
		LocalPlayer.Hotbar.Deserialize(hotbarData);

		Log.Info("Player respawned");

		return LocalPlayer;
	}

	private void UpdateTimer() {
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

	private Vector3 GetRandomEnemySpawn() {
		var pos = LocalPlayer!.GlobalPosition;
		return pos + new Vector3(
			(float) GD.RandRange(-10f, 10f),
			0.25f,
			(float) GD.RandRange(-10f, 10f)
		);

	}

	public void DecrementEnemyCount() {
		EnemyCount -= 1;
	}

	public bool SaveGame(string fileName) {
		if(!InGame) {
			Log.Error("Cannot save game when not in a game");
			return false;
		}

		if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig)) {
			Log.Error("Cannot save game: game objects are not valid");
			return false;
		}

		var data = new GameState {
			Player = LocalPlayer.Serialize(),
			CameraRig = CameraRig.Serialize(),
		};

		SaveService.Save(fileName, data);
		Log.Info($"Game saved to '{fileName}'");
		return true;
	}

	public bool QuickSave() => SaveGame(Constants.AutosaveFile);

	private bool ApplyLoadedState(string fileName) {
		if(!SaveService.Exists(fileName)) {
			Log.Error($"Save file '{fileName}' does not exist");
			return false;
		}

		var data = SaveService.Load<GameState>(fileName);

		LocalPlayer!.Deserialize(data.Player);
		CameraRig!.Deserialize(data.CameraRig);

		Log.Info($"Game loaded from '{fileName}'");
		return true;
	}

	private string? PendingLoadFile;

	public void StartNewGame() {
		Log.Info("Starting new game");
		PendingLoadFile = null;
		TransitionToGame();
	}

	public void ContinueGame() {
		Log.Info("Continuing game from autosave");
		LoadGame(Constants.AutosaveFile);
	}

	public void LoadGame(string fileName) {
		if(!SaveService.Exists(fileName)) {
			Log.Error($"Cannot load game: save file '{fileName}' does not exist");
			return;
		}
		Log.Info($"Loading game from '{fileName}'");
		PendingLoadFile = fileName;
		TransitionToGame();
	}

	private void TransitionToGame() {
		CleanupGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.GameScene);
		GetTree().TreeChanged += OnTreeChangedOnce;
	}

	private void OnTreeChangedOnce() {
		GetTree().TreeChanged -= OnTreeChangedOnce;
		CallDeferred(nameof(OnGameSceneLoaded));
	}

	private void OnGameSceneLoaded() {
		if(!InGame) {
			Log.Warn("OnGameSceneLoaded called but not in game scene yet, deferring...");
			CallDeferred(nameof(OnGameSceneLoaded));
			return;
		}

		SpawnLocalPlayer();

		if(PendingLoadFile != null) {
			ApplyLoadedState(PendingLoadFile);
			PendingLoadFile = null;
		}
		else {
			SpawnTestItems();
		}
	}

	public void ReturnToMainMenu() {
		QuickSave();
		CleanupGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}

	public void ExitApplication() {
		CleanupGame();
		GetTree().Quit();
	}

	private static void CleanupObject(Node? obj) {
		if(IsInstanceValid(obj)) { obj.QueueFree(); }
	}

	private void CleanupGame() {
		CleanupObject(LocalPlayer);
		LocalPlayer = null;

		CleanupObject(Enemy);
		Enemy = null;

		CleanupObject(CameraRig);
		CameraRig = null;

		// Reset spawn state
		EnemyCount = 0;
		SpawnTimer = 5.0f;
	}

	private void SpawnTestItem(string path, Vector3 position, float scaleFactor = 1.0f) {
		Item? item = GD.Load<Item>(path);
		if(item == null) {
			Log.Error($"Failed to load item at path: {path}");
			return;
		}
		Item3DIcon item3DIcon = new Item3DIcon();
		item3DIcon.Item = item;
		item3DIcon.Name = item.Name + "3DIcon";
		AddChild(item3DIcon);
		item3DIcon.ScaleFactor = scaleFactor;
		item3DIcon.SpawnItem3D(position);
	}

	private static Vector3 RandomLocation() {
		return new Vector3(
			GD.RandRange(-SpawnRadius, SpawnRadius),
			SpawnHeight,
			GD.RandRange(-SpawnRadius, SpawnRadius)
		);
	}

	private void SpawnTestItems() {
		SpawnTestItem(Items.AppleRed, RandomLocation());
		SpawnTestItem(Items.AppleYellow, RandomLocation());
		SpawnTestItem(Items.AppleGreen, RandomLocation());
		SpawnTestItem(Items.BananaYellow, RandomLocation());
		SpawnTestItem(Items.BananaGreen, RandomLocation());
		SpawnTestItem(Items.StrawberryGreen, RandomLocation());
		SpawnTestItem(Items.StrawberryRed, RandomLocation());
		SpawnTestItem(Items.StrawberryRed, new Vector3(40, SpawnHeight, 20), 3);
	}
}

namespace SaveSystem {
	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}