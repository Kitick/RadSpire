namespace GameWorld;

using System;
using System.Collections.Generic;
using Camera;
using Character;
using Components;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using ItemSystem;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;
using QuestSystem;
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
	[Export] private PackedScene Item3DIconManagerScene = null!;

	private readonly List<Enemy> SpawnedEnemies = [];
	private readonly List<NPC> SpawnedNPCs = [];
	[Export] private PackedScene WorldObjectManageScene = null!;
	[Export] private PackedScene NPCScene = null!;
	[Export] private Node WorldObjectParentNode = null!;

	private readonly KeyInput KeyInput = new();
	private readonly QuestManager QuestManager = new();
	private CameraRig CameraRig = null!;
	private Player? LocalPlayer;
	private HUD? HUD;
	private Item3DIconManager? Item3DIconManager;
	private WorldObjectManager? WorldObjectManager;
	public Action? MainMenuRequested;

	public enum MenuState { Game, Paused, Settings, Inventory, Chest, Host, Death }
	private readonly StateMachine<MenuState> StateMachine = new(MenuState.Game);

	private string? LoadFile;

	private const int SpawnHeight = 5;
	private const int SpawnRadius = 10;

	[ExportCategory("Spawn Points")]
	[Export] private Marker3D PlayerSpawnMarker = null!;
	[Export] private Marker3D NPCSpawnMarker = null!;
	[Export] private Godot.Collections.Array<Marker3D> EnemySpawnPoints = [];
	[Export] private Godot.Collections.Array<ItemSpawnEntry> ItemSpawnEntries = [];

	public override void _Ready() {
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		CameraRig = this.AddScene<CameraRig>(CameraScene);
		Item3DIconManager = this.AddScene<Item3DIconManager>(Item3DIconManagerScene);
		WorldObjectManager = this.AddScene<WorldObjectManager>(WorldObjectManageScene);
		Node worldRoot = GetParent() ?? this;
		WorldObjectManager.SetUpWorldObjectManager(WorldObjectParentNode, worldRoot);
		AddChild(QuestManager);
		ConfigureStateMachine();

		StartGame();
		ConnectLocationTriggers();
	}

	public override void _ExitTree() {
		DisplaySettings.SetWorldEnvironment(null);
	}

	public override void _PhysicsProcess(double delta) {
		if(!IsInstanceValid(LocalPlayer)) { return; }

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
		NPC npc = this.AddScene<NPC>(NPCScene);
		npc.GlobalPosition = NPCSpawnMarker.GlobalPosition;
		npc.Init(QuestManager);
		SubscribeToNPCEvents(npc);
		SpawnedNPCs.Add(npc);
	}

	private void SpawnLocalPlayer() {
		LocalPlayer = this.AddScene<Player>(PlayerScene);
		LocalPlayer.GlobalPosition = PlayerSpawnMarker.GlobalPosition;

		LocalPlayer.WhenDead(() => StateMachine.TransitionTo(MenuState.Death));

		if(WorldObjectManager != null) {
			LocalPlayer.ConfigureObjectPickup(WorldObjectManager);
		}
		SubscribeToPlayerItem3DIconEvents(LocalPlayer);

		CameraRig.Target = LocalPlayer;
		UpdateEnemyTargets(LocalPlayer);

		QuestManager.Init(LocalPlayer);
		WireEnemyKillEvents();

		AttachHUD();
		LocalPlayer.ConfigureObjectPlacement(WorldObjectManager!, this, HUD!.GetNode<Hotbar>("Hotbar"));
	}

	private void AttachHUD() {
		HUD = HUDScene.Instantiate<HUD>();
		SubscribeToEvents(HUD);
		HUD.Init(LocalPlayer!, StateMachine, QuestManager);
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
		hud.SaveRequested += OnSaveRequested;

		QuestManager.QuestActivated += OnQuestActivated;
		QuestManager.QuestCompleted += OnQuestCompleted;
		QuestManager.StageAdvanced += OnStageAdvanced;
		QuestManager.GameWon += OnGameWon;
	}

	private void OnQuestActivated(QuestID id) => HUD?.ShowQuestNotification($"Quest Started: {id}");
	private void OnQuestCompleted(QuestID id) => HUD?.ShowQuestNotification($"Quest Completed: {id}");
	private void OnStageAdvanced(int stage) => HUD?.ShowQuestNotification($"Stage {stage} reached!");
	private void OnGameWon() => HUD?.Win();

	private void SubscribeToNPCEvents(NPC npc) {
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
			Item3DIconManager = Item3DIconManager!.Export(),
			WorldObjectManager = WorldObjectManager!.Export(),
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
		SpawnItems();
		if(LoadFile != null) {
			LoadData(LoadFile);
			LoadFile = null;
			return;
		}
	}

	private void LoadData(string file) {
		GameState data = SaveService.Load<GameState>(file);

		LocalPlayer!.Import(data.Player);
		CameraRig!.Import(data.CameraRig);
		Item3DIconManager!.Import(data.Item3DIconManager);
		WorldObjectManager!.Import(data.WorldObjectManager);
		QuestManager.Import(data.Progression);
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

		Cleanup(LocalPlayer);
		LocalPlayer = null;

		foreach(NPC npc in SpawnedNPCs) {
			Cleanup(npc);
		}
		SpawnedNPCs.Clear();

		foreach(Enemy enemy in SpawnedEnemies) { Cleanup(enemy); }
		SpawnedEnemies.Clear();

		Cleanup(Item3DIconManager);
		Item3DIconManager = null;
	}

	private void SpawnEnemies() {
		if(EnemySpawnPoints.Count == 0) {
			Log.Info("No EnemySpawnPoints assigned — skipping enemy spawn.");
			return;
		}
		foreach(Marker3D spawnPoint in EnemySpawnPoints) {
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

	private void SpawnItems() {
		if(Item3DIconManager == null) {
			Log.Error("Cannot spawn items: Item3DIconManager is not initialized");
			return;
		}
		if(ItemSpawnEntries.Count == 0) {
			Log.Info("No ItemSpawnEntries assigned — skipping item spawn.");
			return;
		}
		foreach(ItemSpawnEntry entry in ItemSpawnEntries) {
			Item3DIconManager.SpawnItem(entry.ItemId, entry.GlobalPosition);
			Log.Info($"Item '{entry.ItemId}' spawned at {entry.Name} ({entry.GlobalPosition})");
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
		Vector3 center = IsInstanceValid(LocalPlayer) ? LocalPlayer!.GlobalPosition : PlayerSpawnMarker.GlobalPosition;
		Vector3 randomPoint = new(
			center.X + GD.RandRange(-SpawnRadius, SpawnRadius),
			center.Y + SpawnHeight,
			center.Z + GD.RandRange(-SpawnRadius, SpawnRadius)
		);
		return randomPoint;
	}

	private void SubscribeToPlayerItem3DIconEvents(Player player) {
		if(Item3DIconManager == null) { return; }
		player.InventoryManager.SpawnItem3DIconRequested += OnSpawnItem3DIconRequested;
		player.PickupComponent.DespawnItem3DIconRequested += Item3DIconManager.RequestDespawnItem;
	}

	private void OnSpawnItem3DIconRequested(Item item, Vector3 position) =>
		Item3DIconManager?.RequestSpawnItem(item, position);
}

public readonly struct GameState : ISaveData {
	public PlayerData Player { get; init; }
	public CameraRigData CameraRig { get; init; }
	public Item3DIconManagerData Item3DIconManager { get; init; }
	public WorldObjectManagerData WorldObjectManager { get; init; }
	public QuestProgressionData Progression { get; init; }
}
