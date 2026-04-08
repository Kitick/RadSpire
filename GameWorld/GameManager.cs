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

	[Export] private WorldEnvironment? WorldEnvironment;

	[ExportCategory("Scene References")]
	[Export] private PackedScene CameraScene = null!;
	[Export] private PackedScene HUDScene = null!;
	[Export] private PackedScene PlayerScene = null!;
	[Export] private PackedScene EnemyScene = null!;
	[Export] private PackedScene GameWorldManagerScene = null!;

	private readonly List<Enemy> SpawnedEnemies = [];
	[Export] private PackedScene NPCScene = null!;
	[Export] private Node WorldContentRoot = null!;

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
	private Dictionary<string, Vector3> MainWorldReturnPositions = [];
	private Vector3? LastKnownMainWorldPlayerPosition;

	private const int SpawnHeight = 5;
	private const int SpawnRadius = 10;

	[ExportCategory("Spawn Points")]
	[Export] private Marker3D? PlayerSpawnMarker;
	[Export] private Marker3D? NPCSpawnMarker;
	[Export] private Godot.Collections.Array<Marker3D> EnemySpawnPoints = [];

	public override void _Ready() {
		CameraRig = this.AddScene<CameraRig>(CameraScene);
		GameWorldManager = this.AddScene<GameWorldManager>(GameWorldManagerScene);
		Node worldRoot = ResolveWorldRoot();
		GameWorldManager.Initialize(worldRoot, this);
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);
		ConfigureStateMachine();

		StartGame();
	}

	private Node ResolveWorldRoot() {
		if(IsInstanceValid(WorldContentRoot)) {
			return WorldContentRoot;
		}

		Node? fallback = GetNodeOrNull<Node>("Node") ?? GetParent();
		if(fallback != null) {
			Log.Warn("WorldContentRoot is not assigned. Falling back to auto-detected world root node.");
			return fallback;
		}

		Log.Warn("WorldContentRoot is not assigned and no fallback root node was found. Using GameManager as world root.");
		return this;
	}

	private Node? GetActiveWorldNode() {
		if(GameWorldManager?.CurrentGameWorld?.CurrentWorldNode != null && IsInstanceValid(GameWorldManager.CurrentGameWorld.CurrentWorldNode)) {
			return GameWorldManager.CurrentGameWorld.CurrentWorldNode;
		}

		if(!IsInstanceValid(WorldContentRoot) || WorldContentRoot.GetChildCount() == 0) {
			return null;
		}

		return WorldContentRoot.GetChild(0);
	}

	private void RefreshWorldReferences() {
		Node? activeWorld = GetActiveWorldNode();
		if(activeWorld == null) {
			return;
		}

		WorldEnvironment = activeWorld.GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
		PlayerSpawnMarker = activeWorld.GetNodeOrNull<Marker3D>("SpawnLocations/PlayerSpawn") ?? PlayerSpawnMarker;
		NPCSpawnMarker = activeWorld.GetNodeOrNull<Marker3D>("SpawnLocations/NPCSpawn") ?? NPCSpawnMarker;

		Node? spawnLocations = activeWorld.GetNodeOrNull<Node>("SpawnLocations");
		if(spawnLocations == null) {
			return;
		}

		Godot.Collections.Array<Marker3D> markers = [];
		foreach(Node child in spawnLocations.GetChildren()) {
			if(child is Marker3D marker && marker.Name.ToString().StartsWith("Enemy")) {
				markers.Add(marker);
			}
		}

		if(markers.Count > 0) {
			EnemySpawnPoints = markers;
		}
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

		if(GameWorldManager != null && GameWorldManager.CurrentGameWorldId == GameWorldManager.MainGameWorldId) {
			LastKnownMainWorldPlayerPosition = LocalPlayer.GlobalPosition;
		}
	}

	private void ConfigureStateMachine() {
		StateMachine.OnEnter(MenuState.Game, () => GetTree().Paused = false);
		StateMachine.OnExit(MenuState.Game, () => GetTree().Paused = true);

		StateMachine.OnEnter(MenuState.Death, () => GetTree().Paused = false);
	}

	private void SpawnNPC() {
		if(!IsInstanceValid(NPCSpawnMarker)) {
			Log.Warn("NPCSpawn marker not found in active world. Skipping NPC spawn.");
			return;
		}
		var npc = this.AddScene<NPC>(NPCScene);
		npc.GlobalPosition = NPCSpawnMarker.GlobalPosition;
	}

	private void SpawnLocalPlayer() {
		if(!IsInstanceValid(PlayerSpawnMarker)) {
			Log.Error("PlayerSpawn marker not found in active world. Cannot spawn player.");
			return;
		}
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
			MainWorldReturnPositions = new Dictionary<string, Vector3>(MainWorldReturnPositions),
			LastKnownMainWorldPlayerPosition = LastKnownMainWorldPlayerPosition,
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
		MainWorldReturnPositions = data.MainWorldReturnPositions ?? new Dictionary<string, Vector3>();
		LastKnownMainWorldPlayerPosition = data.LastKnownMainWorldPlayerPosition;

		if(LocalPlayer != null) {
			GameWorldManager?.UnbindPlayer(LocalPlayer);
		}

		LocalPlayer!.Import(data.Player);
		CameraRig!.Import(data.CameraRig);
		GameWorldManager!.Import(data.GameWorldManager);
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		if(LocalPlayer != null && GameWorldManager.WorldObjectManager != null && HUD != null) {
			LocalPlayer.ConfigureObjectPickup(GameWorldManager.WorldObjectManager);
			LocalPlayer.ConfigureObjectPlacement(GameWorldManager.WorldObjectManager, this, HUD.GetNode<Hotbar>("Hotbar"));
			GameWorldManager.BindPlayer(LocalPlayer);
		}
	}

	public bool SwitchToGameWorld(string gameWorldId, Vector3? playerSpawnPosition = null) {
		if(GameWorldManager == null) {
			Log.Error("SwitchToGameWorld failed: GameWorldManager is not available.");
			return false;
		}
		if(LocalPlayer == null || !IsInstanceValid(LocalPlayer)) {
			Log.Error("SwitchToGameWorld failed: LocalPlayer is not available.");
			return false;
		}
		if(HUD == null) {
			Log.Error("SwitchToGameWorld failed: HUD is not available.");
			return false;
		}

		GameWorldManager.UnbindPlayer(LocalPlayer);
		if(!GameWorldManager.SwitchToGameWorld(gameWorldId)) {
			GameWorldManager.BindPlayer(LocalPlayer);
			return false;
		}
		
		if(GameWorldManager.WorldObjectManager == null) {
			Log.Error("SwitchToGameWorld failed: active world does not have a WorldObjectManager.");
			GameWorldManager.BindPlayer(LocalPlayer);
			return false;
		}

		LocalPlayer.ConfigureObjectPickup(GameWorldManager.WorldObjectManager);
		LocalPlayer.ConfigureObjectPlacement(GameWorldManager.WorldObjectManager, this, HUD.GetNode<Hotbar>("Hotbar"));
		GameWorldManager.BindPlayer(LocalPlayer);
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		if(playerSpawnPosition.HasValue) {
			LocalPlayer.GlobalPosition = playerSpawnPosition.Value;
		}

		return true;
	}

	public bool TryRecordMainWorldReturnPosition(string destinationWorldId, Vector3 playerPosition) {
		if(GameWorldManager == null) {
			Log.Warn("Cannot record return position: GameWorldManager is not available.");
			return false;
		}
		if(string.IsNullOrEmpty(destinationWorldId)) {
			Log.Warn("Cannot record return position: destination world id is empty.");
			return false;
		}

		string mainWorldId = GameWorldManager.MainGameWorldId;
		if(string.IsNullOrEmpty(mainWorldId) || destinationWorldId == mainWorldId) {
			return false;
		}

		MainWorldReturnPositions[destinationWorldId] = playerPosition;
		LastKnownMainWorldPlayerPosition = playerPosition;
		return true;
	}

	public Vector3? GetMainWorldReturnPosition(string sourceWorldId) {
		if(string.IsNullOrEmpty(sourceWorldId)) {
			return null;
		}

		if(MainWorldReturnPositions.TryGetValue(sourceWorldId, out Vector3 position)) {
			return position;
		}

		return null;
	}

	public Vector3? GetLastKnownMainWorldPlayerPosition() {
		return LastKnownMainWorldPlayerPosition;
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
		MainWorldReturnPositions.Clear();
		LastKnownMainWorldPlayerPosition = null;

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
		RefreshWorldReferences();
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
		Vector3 center = IsInstanceValid(LocalPlayer) ? LocalPlayer!.GlobalPosition : PlayerSpawnMarker?.GlobalPosition ?? Vector3.Zero;
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
	public Dictionary<string, Vector3>? MainWorldReturnPositions { get; init; }
	public Vector3? LastKnownMainWorldPlayerPosition { get; init; }
}
