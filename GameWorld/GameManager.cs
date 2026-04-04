namespace GameWorld;

using System;
using System.Collections.Generic;
using Camera;
using Character;
using Components;
using Godot;
using InventorySystem.Interface;
using ItemSystem.Icons;
using ItemSystem.WorldObjects.House;
using Root;
using Services;
using Settings;
using UI.HUD;

public sealed partial class GameManager : Node {
	private static readonly LogService Log = new(nameof(GameManager), enabled: true);

	[Export] private WorldEnvironment WorldEnvironment = null!;

	[ExportCategory("Scene References")]
	[Export] private PackedScene CameraScene = null!;
	[Export] private PackedScene HUDScene = null!;
	[Export] private PackedScene PlayerScene = null!;
	[Export] private PackedScene EnemyScene = null!;
	[Export] private PackedScene GameWorldManagerScene = null!;

	private readonly List<Enemy> SpawnedEnemies = [];
	[Export] private PackedScene NPCScene = null!;

	private readonly KeyInput KeyInput = new();
	private CameraRig CameraRig = null!;
	private Player? LocalPlayer;
	private HUD? HUD;
	private GameWorldManager? GameWorldManager;
	public Action? MainMenuRequested;

	public enum MenuState { Game, Paused, Settings, Inventory, Chest, Host, Death }
	private readonly StateMachine<MenuState> StateMachine = new(MenuState.Game);

	private string? LoadFile;
	private bool Won = false;

	private const int SpawnHeight = 5;
	private const int SpawnRadius = 10;

	[ExportCategory("Spawn Points")]
	[Export] private Marker3D PlayerSpawnMarker = null!;
	[Export] private Marker3D NPCSpawnMarker = null!;
	[Export] private Godot.Collections.Array<Marker3D> EnemySpawnPoints = [];

	public override void _Ready() {
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		CameraRig = this.AddScene<CameraRig>(CameraScene);
		GameWorldManager = this.AddScene<GameWorldManager>(GameWorldManagerScene);
		GameWorldManager.Initialize(GetParent() ?? this);
		ConfigureStateMachine();

		StartGame();
	}

	public override void _ExitTree() {
		DisplaySettings.SetWorldEnvironment(null);
	}

	public override void _PhysicsProcess(double delta) {
		if(!IsInstanceValid(LocalPlayer)) { return; }

		if(!Won) {
			int killed = 0;
			foreach(var enemy in SpawnedEnemies) {
				if(enemy.Health.Current == 0) {
					killed += 1;
				}
			}

			if(killed == SpawnedEnemies.Count) { HUD?.Win(); }
		}

		float dt = (float) delta;

		KeyInput.Update(CameraRig);
		LocalPlayer.Update(dt, KeyInput);
	}

	private void ConfigureStateMachine() {
		StateMachine.OnEnter(MenuState.Game, () => GetTree().Paused = false);
		StateMachine.OnExit(MenuState.Game, () => GetTree().Paused = true);

		StateMachine.OnEnter(MenuState.Death, () => GetTree().Paused = false);
	}

	private void SpawnNPC() {
		var npc = this.AddScene<NPC>(NPCScene);
		npc.GlobalPosition = NPCSpawnMarker.GlobalPosition;
	}

	private void SpawnLocalPlayer() {
		LocalPlayer = this.AddScene<Player>(PlayerScene);
		LocalPlayer.GlobalPosition = PlayerSpawnMarker.GlobalPosition;

		LocalPlayer.WhenDead(() => StateMachine.TransitionTo(MenuState.Death));

		if(GameWorldManager?.WorldObjectManager != null) {
			LocalPlayer.ConfigureObjectPickup(GameWorldManager.WorldObjectManager);
		}
		GameWorldManager?.BindPlayer(LocalPlayer);

		CameraRig.Target = LocalPlayer;
		UpdateEnemyTargets(LocalPlayer);

		AttachHUD();
		LocalPlayer.ConfigureObjectPlacement(GameWorldManager!.WorldObjectManager!, this, HUD!.GetNode<Hotbar>("Hotbar"));
	}

	private void AttachHUD() {
		HUD = HUDScene.Instantiate<HUD>();
		SubscribeToEvents(HUD);
		HUD.Init(LocalPlayer!, StateMachine);
		Hotbar hotbar = HUD.GetNode<Hotbar>("Hotbar");
		LocalPlayer!.UseItemComponent.UserHotbar = hotbar;
		LocalPlayer.EquipItemComponent.Initalize(LocalPlayer, hotbar);

		AddChild(HUD);
	}

	private void SubscribeToEvents(HUD hud) {
		hud.PauseRequested += () => StateMachine.TransitionTo(MenuState.Paused);
		hud.ResumeRequested += () => StateMachine.TransitionTo(MenuState.Game);
		hud.SettingsRequested += () => StateMachine.TransitionTo(MenuState.Settings);
		hud.HostRequested += () => StateMachine.TransitionTo(MenuState.Host);
		hud.MainMenuRequested += ReturnToMainMenu;
		hud.RespawnRequested += RespawnPlayer;
		hud.SaveRequested += (fileName) => SaveGame(fileName);
	}

	public void RespawnPlayer() {
		if(!IsInstanceValid(LocalPlayer)) {
			Log.Warn("RespawnPlayer called but no player exists, spawning new player");
			SpawnLocalPlayer();
			return;
		}

		var inventoryData = LocalPlayer.Inventory.Export();
		var hotbarData = LocalPlayer.Hotbar.Export();

		GameWorldManager?.UnbindPlayer(LocalPlayer);
		LocalPlayer.QueueFree();
		LocalPlayer = null;

		SpawnLocalPlayer();

		LocalPlayer!.Inventory.Import(inventoryData);
		LocalPlayer.Hotbar.Import(hotbarData);

		StateMachine.TransitionTo(MenuState.Game);

		Log.Info("Player respawned");
	}

	public bool SaveGame(string fileName) {
		if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig) || GameWorldManager == null) {
			Log.Error("Cannot save game: game objects are not valid");
			return false;
		}

		var data = new GameState {
			Player = LocalPlayer.Export(),
			CameraRig = CameraRig.Export(),
			GameWorldManager = GameWorldManager.Export(),
		};

		data.Save(fileName);

		Log.Info($"Game saved to '{fileName}'");
		return true;
	}

	public bool QuickSave() => SaveGame(Constants.AutosaveFile);

	public void InitGame(string? loadfile = null) {
		Log.Info("Initializing game");

		if(loadfile != null && !SaveService.Exists(loadfile)) {
			Log.Error($"Cannot initialize game: save file '{loadfile}' does not exist");
		}
		else {
			LoadFile = loadfile;
		}
	}

	private void StartGame() {
		SpawnLocalPlayer();
		SpawnNPC();
		SpawnEnemies();
		if(LoadFile != null) {
			LoadData(LoadFile);
			LoadFile = null;
			return;
		}
	}

	private void LoadData(string file) {
		var data = SaveService.Load<GameState>(file);

		LocalPlayer!.Import(data.Player);
		CameraRig!.Import(data.CameraRig);
		GameWorldManager!.Import(data.GameWorldManager);
	}

	public void ReturnToMainMenu() {
		QuickSave();
		CleanupGame();
		GetTree().Paused = false;
		MainMenuRequested?.Invoke();
	}

	private static void Cleanup(Node? node) {
		if(IsInstanceValid(node)) { node.QueueFree(); }
	}

	private void CleanupGame() {
		HUD = null;

		if(IsInstanceValid(LocalPlayer)) {
			GameWorldManager?.UnbindPlayer(LocalPlayer!);
		}

		Cleanup(LocalPlayer);
		LocalPlayer = null;

		GameWorldManager?.Cleanup();
		GameWorldManager = null;

		foreach(var enemy in SpawnedEnemies) { Cleanup(enemy); }
		SpawnedEnemies.Clear();
	}

	private void SpawnEnemies() {
		if(EnemySpawnPoints.Count == 0) {
			Log.Info("No EnemySpawnPoints assigned — skipping enemy spawn.");
			return;
		}
		foreach(var spawnPoint in EnemySpawnPoints) {
			if(!IsInstanceValid(spawnPoint)) continue;
			var enemy = this.AddScene<Enemy>(EnemyScene);
			enemy.GlobalPosition = spawnPoint.GlobalPosition;
			if(LocalPlayer != null) enemy.SetTarget(LocalPlayer);
			SpawnedEnemies.Add(enemy);
			Log.Info($"Enemy spawned at {spawnPoint.Name} ({spawnPoint.GlobalPosition})");
		}
	}

	private void UpdateEnemyTargets(Node3D target) {
		foreach(var enemy in SpawnedEnemies) {
			if(IsInstanceValid(enemy)) enemy.SetTarget(target);
		}
	}

	private Vector3 RandomLocationNearPlayer() {
		Vector3 center = IsInstanceValid(LocalPlayer) ? LocalPlayer!.GlobalPosition : PlayerSpawnMarker.GlobalPosition;
		Vector3 randomPoint = new Vector3(
			center.X + GD.RandRange(-SpawnRadius, SpawnRadius),
			center.Y + SpawnHeight,
			center.Z + GD.RandRange(-SpawnRadius, SpawnRadius)
		);
		return randomPoint;
	}

}

public readonly struct GameState : ISaveData {
	public PlayerData Player { get; init; }
	public CameraRigData CameraRig { get; init; }
	public GameWorldManagerData GameWorldManager { get; init; }
}
