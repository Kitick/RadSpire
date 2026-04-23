namespace GameWorld;

using System;
using System.Collections.Generic;
using Camera;
using Character;
using Components;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using QuestSystem;
using Root;
using Services;
using Settings;
using UI.HUD;

public sealed partial class GameManager : Node {
	private static readonly LogService Log = new(nameof(GameManager), enabled: true);

	[ExportCategory("Scene References")]
	[Export] private PackedScene CameraScene = null!;
	[Export] private PackedScene HUDScene = null!;
	[Export] private PackedScene PlayerScene = null!;
	[Export] private PackedScene EnemyScene = null!;
	[Export] private PackedScene NPCScene = null!;

	[ExportCategory("Worlds")]
	[Export] private Outside OutsideWorld = null!;

	private readonly List<Enemy> SpawnedEnemies = [];
	private readonly List<NPC> SpawnedNPCs = [];

	private readonly KeyInput KeyInput = new();
	private readonly QuestManager QuestManager = new();
	private CameraRig CameraRig = null!;
	private Player? LocalPlayer;
	private HUD? HUD;
	public Action? MainMenuRequested;

	public enum MenuState { Game, Paused, Settings, Inventory, Chest, Host, Death }
	private readonly StateMachine<MenuState> StateMachine = new(MenuState.Game);

	private string? LoadFile;
	private readonly bool Won = false;

	private const int SpawnHeight = 5;
	private const int SpawnRadius = 10;

	public override void _Ready() {
		this.ValidateExports();
		CameraRig = this.AddScene<CameraRig>(CameraScene);
		DisplaySettings.SetWorldEnvironment(OutsideWorld.WorldEnvironment);
		OutsideWorld.WorldObjectManager?.SetUpWorldObjectManager(OutsideWorld, OutsideWorld, this);
		AddChild(QuestManager);
		ConfigureStateMachine();
		StartGame();
		ConnectLocationTriggers();
	}

	public override void _ExitTree() => DisplaySettings.SetWorldEnvironment(null);

	public override void _PhysicsProcess(double delta) {
		if(!IsInstanceValid(LocalPlayer)) { return; }

		if(!Won && SpawnedEnemies.Count > 0) {
			int killed = 0;
			foreach(Enemy enemy in SpawnedEnemies) {
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
		if(!IsInstanceValid(OutsideWorld.NPCSpawnMarker)) {
			Log.Warn("NPCSpawnMarker not set on OutsideWorld. Skipping NPC spawn.");
			return;
		}
		NPC npc = this.AddScene<NPC>(NPCScene);
		npc.GlobalPosition = OutsideWorld.NPCSpawnMarker.GlobalPosition;
		npc.Init(QuestManager);
		SubscribeToEvents(npc);
		SpawnedNPCs.Add(npc);
	}

	private void SpawnLocalPlayer() {
		if(!IsInstanceValid(OutsideWorld.PlayerSpawnMarker)) {
			Log.Error("PlayerSpawnMarker not set on OutsideWorld. Cannot spawn player.");
			return;
		}
		LocalPlayer = this.AddScene<Player>(PlayerScene);
		LocalPlayer.GlobalPosition = OutsideWorld.PlayerSpawnMarker.GlobalPosition;
		LocalPlayer.WhenDead(() => StateMachine.TransitionTo(MenuState.Death));
		if(OutsideWorld.WorldObjectManager != null) {
			LocalPlayer.ConfigureObjectPickup(OutsideWorld.WorldObjectManager);
		}
		CameraRig.Target = LocalPlayer;
		UpdateEnemyTargets(LocalPlayer);
		QuestManager.Init(LocalPlayer);
		WireEnemyKillEvents();
		AttachHUD();
		LocalPlayer.ConfigureObjectPlacement(OutsideWorld.WorldObjectManager!, this, HUD!.Hotbar);
	}

	private void AttachHUD() {
		HUD = HUDScene.Instantiate<HUD>();
		AddChild(HUD);
		SubscribeToEvents(HUD);
		HUD.Init(LocalPlayer!, StateMachine, QuestManager);
		Hotbar hotbar = HUD.Hotbar;
		LocalPlayer!.UseItemComponent.UserHotbar = hotbar;
		LocalPlayer.EquipItemComponent.Initalize(LocalPlayer, hotbar);
	}

	private void SubscribeToEvents(HUD hud) {
		hud.PauseRequested += () => StateMachine.TransitionTo(MenuState.Paused);
		hud.ResumeRequested += () => StateMachine.TransitionTo(MenuState.Game);
		hud.SettingsRequested += () => StateMachine.TransitionTo(MenuState.Settings);
		hud.HostRequested += () => StateMachine.TransitionTo(MenuState.Host);
		hud.MainMenuRequested += ReturnToMainMenu;
		hud.RespawnRequested += RespawnPlayer;
		hud.SaveRequested += OnSaveRequested;

		QuestManager.QuestActivated += OnQuestActivated;
		QuestManager.QuestCompleted += OnQuestCompleted;
		QuestManager.StageAdvanced += OnStageAdvanced;
		QuestManager.GameWon += OnGameWon;
	}

	private void OnQuestActivated(QuestID id) => Log.Info($"Quest activated: {id}");
	private void OnQuestCompleted(QuestID id) => HUD?.ShowQuestNotification($"Quest Completed: {id}");
	private void OnStageAdvanced(int stage) => Log.Info($"Stage advanced to {stage}");
	private void OnGameWon() => HUD?.Win();

	private void SubscribeToEvents(NPC npc) {
		npc.Talked += QuestManager.NotifyPlayerTalkedToNPC;
		npc.InteractionPromptChanged += OnNPCInteractionPromptChanged;
	}

	private void OnSaveRequested(string fileName) => SaveGame(fileName);

	private void OnNPCInteractionPromptChanged(string? prompt) {
		if(prompt == null) { HUD?.HideInteractionPrompt(); } else { HUD?.ShowInteractionPrompt(prompt); }
	}

	public void RespawnPlayer() {
		if(!IsInstanceValid(LocalPlayer)) {
			Log.Warn("RespawnPlayer called but no player exists, spawning new player");
			SpawnLocalPlayer();
			return;
		}

		InventoryData inventoryData = LocalPlayer.Inventory.Export();
		InventoryData hotbarData = LocalPlayer.Hotbar.Export();

		LocalPlayer.QueueFree();
		LocalPlayer = null;

		SpawnLocalPlayer();

		LocalPlayer!.Inventory.Import(inventoryData);
		LocalPlayer.Hotbar.Import(hotbarData);

		StateMachine.TransitionTo(MenuState.Game);
		Log.Info("Player respawned");
	}

	public bool SaveGame(string fileName) {
		if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig)) {
			Log.Error("Cannot save game: game objects are not valid");
			return false;
		}

		GameState data = new() {
			Player = LocalPlayer.Export(),
			CameraRig = CameraRig.Export(),
			Progression = QuestManager.Export(),
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
		} else {
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
		}
	}

	private void LoadData(string file) {
		GameState data = SaveService.Load<GameState>(file);

		LocalPlayer!.Import(data.Player);
		CameraRig!.Import(data.CameraRig);
		QuestManager.Import(data.Progression);

		if(LocalPlayer != null && OutsideWorld.WorldObjectManager != null && HUD != null) {
			LocalPlayer.ConfigureObjectPickup(OutsideWorld.WorldObjectManager);
			LocalPlayer.ConfigureObjectPlacement(OutsideWorld.WorldObjectManager, this, HUD.Hotbar);
		}
	}

	public void SwitchToOutside(Vector3? spawnPosition = null) => Log.Info("TODO: SwitchToOutside — world switching not yet implemented in new architecture.");

	public void SwitchToBuilding(Vector3? spawnPosition = null) => Log.Info("TODO: SwitchToBuilding — world switching not yet implemented in new architecture.");

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

		Cleanup(LocalPlayer);
		LocalPlayer = null;

		foreach(NPC npc in SpawnedNPCs) { Cleanup(npc); }
		SpawnedNPCs.Clear();

		foreach(Enemy enemy in SpawnedEnemies) { Cleanup(enemy); }
		SpawnedEnemies.Clear();
	}

	private void SpawnEnemies() {
		if(OutsideWorld.EnemySpawnPoints.Count == 0) {
			Log.Info("No EnemySpawnPoints on OutsideWorld — skipping enemy spawn.");
			return;
		}
		foreach(Marker3D spawnPoint in OutsideWorld.EnemySpawnPoints) {
			if(!IsInstanceValid(spawnPoint)) { continue; }

			Enemy enemy = this.AddScene<Enemy>(EnemyScene);
			enemy.GlobalPosition = spawnPoint.GlobalPosition;
			if(LocalPlayer != null) {
				enemy.SetTarget(LocalPlayer);
			}

			SpawnedEnemies.Add(enemy);
			Log.Info($"Enemy spawned at {spawnPoint.Name} ({spawnPoint.GlobalPosition})");
		}
	}

	private void UpdateEnemyTargets(Node3D target) {
		foreach(Enemy enemy in SpawnedEnemies) {
			if(IsInstanceValid(enemy)) {
				enemy.SetTarget(target);
			}
		}
	}

	private void WireEnemyKillEvents() {
		foreach(Enemy enemy in SpawnedEnemies) {
			if(!IsInstanceValid(enemy)) { continue; }
			enemy.WhenDead(() => QuestManager.NotifyEnemyKilled(enemy.EnemyType));
		}
	}

	private void ConnectLocationTriggers() {
		foreach(Node node in GetTree().GetNodesInGroup(Group.QuestLocation.ToString())) {
			if(node is QuestLocationTrigger trigger) {
				trigger.PlayerReachedLocation += QuestManager.NotifyLocationReached;
			}
		}
	}

	private Vector3 RandomLocationNearPlayer() {
		Vector3 center = IsInstanceValid(LocalPlayer) ? LocalPlayer.GlobalPosition : OutsideWorld.PlayerSpawnMarker?.GlobalPosition ?? Vector3.Zero;
		Vector3 randomPoint = new(
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
	public QuestProgressionData Progression { get; init; }
}
