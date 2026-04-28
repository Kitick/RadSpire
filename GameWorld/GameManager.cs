namespace GameWorld;

using System;
using System.Collections.Generic;
using Camera;
using Character;
using Components;
using Godot;
using InventorySystem.Interface;
using ItemSystem.WorldObjects.House;
using QuestSystem;
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
	[Export] private PackedScene NPCScene = null!;
	[Export] private Node WorldContentRoot = null!;
	[ExportCategory("Audio")]
	[Export] private AudioStream? GameWorldMusic = GD.Load<AudioStream>("res://Assets/Audio/Candlelit Keep.wav");

	private readonly KeyInput KeyInput = new();
	private readonly QuestManager QuestManager = new();
	private CameraRig CameraRig = null!;
	private Player? LocalPlayer;
	private HUD? HUD;
	private GameWorldManager? WorldManager;
	private AudioStreamPlayer? GameWorldMusicPlayer;
	public Action? MainMenuRequested;

	public enum MenuState { Game, Paused, Settings, Inventory, Chest, Build, Host, Death }
	private readonly StateMachine<MenuState> StateMachine = new(MenuState.Game);

	private Action? OnExit;
	private string? LoadFile;
	private readonly bool Won = false;
	private Dictionary<string, Vector3> MainWorldReturnPositions = [];
	private Vector3? LastKnownMainWorldPlayerPosition;

	[ExportCategory("Spawn Points")]
	[Export] private Marker3D? PlayerSpawnMarker;

	public PackedScene EnemySceneRef => EnemyScene;
	public PackedScene NPCSceneRef => NPCScene;
	public QuestManager QuestManagerRef => QuestManager;
	public HUD? HUDRef => HUD;
	public GameWorldManager? GameWorldManagerRef => WorldManager;

	public override void _Ready() {
		CameraRig = this.AddScene<CameraRig>(CameraScene);
		WorldManager = this.AddScene<GameWorldManager>(GameWorldManagerScene);
		WorldManager.Initialize(WorldContentRoot, this);
		InitializeAudio();
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);
		AddChild(QuestManager);
		ConfigureStateMachine();

		StartGame();
		ConnectLocationTriggers();
		UpdateGameWorldMusic();
	}

	private void InitializeAudio() {
		GameWorldMusicPlayer = new AudioStreamPlayer {
			Name = "GameWorldMusicPlayer",
			Bus = "Music",
			VolumeDb = 0.0f,
			Stream = GameWorldMusic,
			Autoplay = false,
		};

		AddChild(GameWorldMusicPlayer);
	}

	private Node? GetActiveWorldNode() {
		if(WorldManager?.CurrentGameWorld?.CurrentWorldNode != null && IsInstanceValid(WorldManager.CurrentGameWorld.CurrentWorldNode)) {
			return WorldManager.CurrentGameWorld.CurrentWorldNode;
		}

		return null;
	}

	private void RefreshWorldReferences() {
		Node? activeWorld = GetActiveWorldNode();
		if(activeWorld == null) {
			return;
		}

		if(activeWorld is not Outside outsideWorld) {
			WorldEnvironment = activeWorld.GetNodeOrNull<WorldEnvironment>("WorldEnvironment");
			return;
		}

		WorldEnvironment = outsideWorld.WorldEnvironment;
		PlayerSpawnMarker = outsideWorld.PlayerSpawnMarker;
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		DisplaySettings.SetWorldEnvironment(null);
	}

	public override void _PhysicsProcess(double delta) {
		if(!IsInstanceValid(LocalPlayer)) { return; }

		if(!Won && WorldManager?.EnemyManager != null && WorldManager.EnemyManager.Enemies.Count > 0) {
			int killed = 0;
			foreach(Enemy enemy in WorldManager.EnemyManager.Enemies.Values) {
				if(IsInstanceValid(enemy) && enemy.Health.Current == 0) {
					killed += 1;
				}
			}

			if(killed == WorldManager.EnemyManager.Enemies.Count) { HUD?.Win(); }
		}

		float dt = (float) delta;

		KeyInput.Update(CameraRig);
		LocalPlayer.Update(dt, KeyInput);

		if(WorldManager != null && WorldManager.CurrentGameWorldId == WorldManager.MainGameWorldId) {
			LastKnownMainWorldPlayerPosition = LocalPlayer.GlobalPosition;
		}
	}

	private void ConfigureStateMachine() {
		StateMachine.OnEnter(MenuState.Game, () => GetTree().Paused = false);
		StateMachine.OnExit(MenuState.Game, () => GetTree().Paused = true);
		StateMachine.OnEnter(MenuState.Build, () => GetTree().Paused = false);
		StateMachine.OnEnter(MenuState.Death, () => GetTree().Paused = false);
	}

	private void SpawnLocalPlayer() {
		if(!IsInstanceValid(PlayerSpawnMarker)) {
			Log.Error("PlayerSpawn marker not found in active world. Cannot spawn player.");
			return;
		}
		LocalPlayer = this.AddScene<Player>(PlayerScene);
		LocalPlayer.GlobalPosition = PlayerSpawnMarker.GlobalPosition;

		OnExit += LocalPlayer.WhenDead(() => StateMachine.TransitionTo(MenuState.Death));

		if(WorldManager?.WorldObjectManager != null) {
			LocalPlayer.ConfigureObjectPickup(WorldManager.WorldObjectManager);
		}
		WorldManager?.BindPlayer(LocalPlayer);

		CameraRig.Target = LocalPlayer;
		QuestManager.Init(LocalPlayer);

		AttachHUD();
		LocalPlayer.ConfigureObjectPlacement(WorldManager!.WorldObjectManager!, this, HUD!.Hotbar);
		SyncActiveWorldActorBindings();
	}

	private void AttachHUD() {
		HUD = HUDScene.Instantiate<HUD>();
		SubscribeToEvents(HUD);
		HUD.Init(LocalPlayer!, StateMachine, QuestManager);
		HUD.BindStructureInfo(WorldManager);
		Hotbar hotbar = HUD.Hotbar;
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

	private void OnSaveRequested(string fileName) => SaveGame(fileName);


	public void RespawnPlayer() {
		if(!IsInstanceValid(LocalPlayer)) {
			Log.Warn("RespawnPlayer called but no player exists, spawning new player");
			SpawnLocalPlayer();
			return;
		}

		LocalPlayer.Heal();
		LocalPlayer.Radiation.Level = 0f;
		LocalPlayer.GlobalPosition = PlayerSpawnMarker!.GlobalPosition;
		LocalPlayer.Velocity = Vector3.Zero;

		StateMachine.TransitionTo(MenuState.Game);

		Log.Info("Player respawned");
	}

	public bool SaveGame(string fileName) {
		if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig) || WorldManager == null) {
			Log.Error("Cannot save game: game objects are not valid");
			return false;
		}

		GameState data = new() {
			Player = LocalPlayer.Export(),
			CameraRig = CameraRig.Export(),
			GameWorldManager = WorldManager.Export(),
			MainWorldReturnPositions = new Dictionary<string, Vector3>(MainWorldReturnPositions),
			LastKnownMainWorldPlayerPosition = LastKnownMainWorldPlayerPosition,
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
		}
		else {
			LoadFile = loadfile;
		}
	}

	private void StartGame() {
		SpawnLocalPlayer();
		if(LoadFile != null) {
			LoadData(LoadFile);
			LoadFile = null;
			return;
		}
	}

	private void LoadData(string file) {
		GameState data = SaveService.Load<GameState>(file);
		MainWorldReturnPositions = data.MainWorldReturnPositions ?? [];
		LastKnownMainWorldPlayerPosition = data.LastKnownMainWorldPlayerPosition;

		if(LocalPlayer != null) {
			WorldManager?.UnbindPlayer(LocalPlayer);
		}

		LocalPlayer!.Import(data.Player);
		CameraRig!.Import(data.CameraRig);
		WorldManager!.Import(data.GameWorldManager);
		QuestManager.Import(data.Progression);
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		if(LocalPlayer != null && WorldManager.WorldObjectManager != null && HUD != null) {
			LocalPlayer.ConfigureObjectPickup(WorldManager.WorldObjectManager);
			LocalPlayer.ConfigureObjectPlacement(WorldManager.WorldObjectManager, this, HUD.Hotbar);
			WorldManager.BindPlayer(LocalPlayer);
			SyncActiveWorldActorBindings();
		}

		UpdateGameWorldMusic();
	}

	public bool SwitchToGameWorld(string gameWorldId, Vector3? playerSpawnPosition = null) {
		if(WorldManager == null) {
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

		string currentWorldId = WorldManager.CurrentGameWorldId;
		string mainWorldId = WorldManager.MainGameWorldId;
		Vector3? resolvedSpawnPosition = playerSpawnPosition;
		if(!string.IsNullOrEmpty(mainWorldId) && currentWorldId == mainWorldId && gameWorldId != mainWorldId) {
			bool recorded = TryRecordMainWorldReturnPosition(gameWorldId, LocalPlayer.GlobalPosition);
			Log.Info($"Recorded main-world return for '{gameWorldId}': {recorded}, pos={LocalPlayer.GlobalPosition}");
		}
		else if(!string.IsNullOrEmpty(mainWorldId)
			&& gameWorldId == mainWorldId
			&& currentWorldId != mainWorldId
			&& !resolvedSpawnPosition.HasValue) {
			resolvedSpawnPosition = GetMainWorldReturnPosition(currentWorldId) ?? GetLastKnownMainWorldPlayerPosition();
			if(resolvedSpawnPosition.HasValue) {
			}
			else {
				Log.Warn($"No return-to-main spawn found for world '{currentWorldId}'. Using current player position.");
			}
		}

		WorldManager.UnbindPlayer(LocalPlayer);
		if(!WorldManager.SwitchToGameWorld(gameWorldId)) {
			WorldManager.BindPlayer(LocalPlayer);
			return false;
		}

		if(WorldManager.WorldObjectManager == null) {
			Log.Error("SwitchToGameWorld failed: active world does not have a WorldObjectManager.");
			WorldManager.BindPlayer(LocalPlayer);
			return false;
		}

		LocalPlayer.ConfigureObjectPickup(WorldManager.WorldObjectManager);
		LocalPlayer.ConfigureObjectPlacement(WorldManager.WorldObjectManager, this, HUD.Hotbar);
		WorldManager.BindPlayer(LocalPlayer);
		RefreshWorldReferences();
		DisplaySettings.SetWorldEnvironment(WorldEnvironment);

		if(resolvedSpawnPosition.HasValue) {
			LocalPlayer.GlobalPosition = resolvedSpawnPosition.Value;
		}
		if(gameWorldId == WorldManager.MainGameWorldId) {
			WorldManager.CurrentStructureObject = null;
		}
		LocalPlayer.Velocity = Vector3.Zero;
		SyncActiveWorldActorBindings();
		UpdateGameWorldMusic();

		return true;
	}

	private void UpdateGameWorldMusic() {
		if(GameWorldMusicPlayer == null || !IsInstanceValid(GameWorldMusicPlayer)) {
			return;
		}

		GameWorldMusicPlayer.Stream ??= GameWorldMusic;

		bool inMainGameWorld = WorldManager != null
			&& !string.IsNullOrEmpty(WorldManager.CurrentGameWorldId)
			&& WorldManager.CurrentGameWorldId == WorldManager.MainGameWorldId;

		if(inMainGameWorld) {
			if(!GameWorldMusicPlayer.Playing) {
				GameWorldMusicPlayer.Play();
			}
			return;
		}

		if(GameWorldMusicPlayer.Playing) {
			GameWorldMusicPlayer.Stop();
		}
	}

	public bool TryRecordMainWorldReturnPosition(string destinationWorldId, Vector3 playerPosition) {
		if(WorldManager == null) {
			Log.Warn("Cannot record return position: GameWorldManager is not available.");
			return false;
		}
		if(string.IsNullOrEmpty(destinationWorldId)) {
			Log.Warn("Cannot record return position: destination world id is empty.");
			return false;
		}

		string mainWorldId = WorldManager.MainGameWorldId;
		if(string.IsNullOrEmpty(mainWorldId) || destinationWorldId == mainWorldId) {
			return false;
		}

		MainWorldReturnPositions[destinationWorldId] = playerPosition;
		LastKnownMainWorldPlayerPosition = playerPosition;
		Log.Info($"Main-world return position recorded for '{destinationWorldId}': {playerPosition}");
		return true;
	}

	public Vector3? GetMainWorldReturnPosition(string sourceWorldId) {
		if(string.IsNullOrEmpty(sourceWorldId)) {
			return null;
		}

		if(MainWorldReturnPositions.TryGetValue(sourceWorldId, out Vector3 position)) {
			Log.Info($"Found main-world return position for '{sourceWorldId}': {position}");
			return position;
		}

		Log.Info($"No main-world return position found for '{sourceWorldId}'.");
		return null;
	}

	public Vector3? GetLastKnownMainWorldPlayerPosition() => LastKnownMainWorldPlayerPosition;

	public bool IsPlayerInInteriorWorld() {
		if(WorldManager == null) {
			return false;
		}
		if(string.IsNullOrEmpty(WorldManager.CurrentGameWorldId) || string.IsNullOrEmpty(WorldManager.MainGameWorldId)) {
			return false;
		}
		return WorldManager.CurrentGameWorldId != WorldManager.MainGameWorldId;
	}

	private void SyncActiveWorldActorBindings() {
		if(WorldManager == null || LocalPlayer == null || !IsInstanceValid(LocalPlayer)) {
			return;
		}

		WorldManager.EnemyManager?.SetTarget(LocalPlayer);
		WorldManager.EnemyManager?.BindQuestEvents(QuestManager);
		WorldManager.NPCManager?.UnbindPromptForwarder();
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
		if(GameWorldMusicPlayer != null && IsInstanceValid(GameWorldMusicPlayer) && GameWorldMusicPlayer.Playing) {
			GameWorldMusicPlayer.Stop();
		}

		HUD = null;
		MainWorldReturnPositions.Clear();
		LastKnownMainWorldPlayerPosition = null;
		WorldManager?.EnemyManager?.UnbindQuestEvents();
		WorldManager?.NPCManager?.UnbindPromptForwarder();

		if(IsInstanceValid(LocalPlayer)) {
			WorldManager?.UnbindPlayer(LocalPlayer);
		}

		Cleanup(LocalPlayer);
		LocalPlayer = null;

		WorldManager?.Cleanup();
		WorldManager = null;
	}

	private void ConnectLocationTriggers() {
		foreach(Node node in GetTree().GetNodesInGroup(Group.QuestLocation.ToString())) {
			if(node is QuestLocationTrigger trigger) {
				trigger.PlayerReachedLocation += QuestManager.NotifyLocationReached;
			}
		}
	}

}

public readonly struct GameState : ISaveData {
	public PlayerData Player { get; init; }
	public CameraRigData CameraRig { get; init; }
	public GameWorldManagerData GameWorldManager { get; init; }
	public Dictionary<string, Vector3>? MainWorldReturnPositions { get; init; }
	public Vector3? LastKnownMainWorldPlayerPosition { get; init; }
	public QuestProgressionData Progression { get; init; }
}
