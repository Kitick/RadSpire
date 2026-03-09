namespace Root {
	using System;
	using Camera;
	using Character;
	using Components;
	using Core;
	using Godot;
	using ItemSystem;
	using Objects;
	using Services;
	using Services.Settings;
	using UI;

	public sealed partial class GameManager : Node {
		private static readonly LogService Log = new(nameof(GameManager), enabled: true);

		[Export] private WorldEnvironment WorldEnvironment = null!;

		[ExportCategory("Scene References")]
		[Export] private PackedScene CameraScene = null!;
		[Export] private PackedScene HUDScene = null!;
		[Export] private PackedScene PlayerScene = null!;
		[Export] private PackedScene EnemyScene = null!;
		[Export] private PackedScene Item3DIconManagerScene = null!;

		private EnemySpawner EnemySpawner = null!;
		[Export] private PackedScene WorldObjectManageScene = null!;
		[Export] private PackedScene NPCScene = null!;
		[Export] private Node WorldObjectParentNode = null!;

		private readonly KeyInput KeyInput = new();
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

		public override void _Ready() {
			DisplaySettings.SetWorldEnvironment(WorldEnvironment);

			CameraRig = this.AddScene<CameraRig>(CameraScene);
			Item3DIconManager = this.AddScene<Item3DIconManager>(Item3DIconManagerScene);
			WorldObjectManager = this.AddScene<WorldObjectManager>(WorldObjectManageScene);
			Node worldRoot = GetParent() ?? this;
			WorldObjectManager.SetUpWorldObjectManager(WorldObjectParentNode, worldRoot);
			EnemySpawner = new EnemySpawner(this, EnemyScene);
			ConfigureStateMachine();

			StartGame();
		}

		public override void _ExitTree() {
			DisplaySettings.SetWorldEnvironment(null);
		}

		public override void _PhysicsProcess(double delta) {
			if(!IsInstanceValid(LocalPlayer)) { return; }

			float dt = (float) delta;

			KeyInput.Update(CameraRig);
			LocalPlayer.Update(dt, KeyInput);

			EnemySpawner.Update();
		}

		private void ConfigureStateMachine() {
			StateMachine.OnEnter(MenuState.Game, () => GetTree().Paused = false);
			StateMachine.OnExit(MenuState.Game, () => GetTree().Paused = true);
		}

		private void SpawnNPC() {
			var npc = this.AddScene<NPC>(NPCScene);
			npc.GlobalPosition = NPCSpawnMarker.GlobalPosition;
		}

		private void SpawnLocalPlayer() {
			LocalPlayer = this.AddScene<Player>(PlayerScene);
			LocalPlayer.GlobalPosition = PlayerSpawnMarker.GlobalPosition;

			if(WorldObjectManager != null) {
				LocalPlayer.ConfigureObjectPickup(WorldObjectManager);
			}
			SubscribeToPlayerItem3DIconEvents(LocalPlayer);

			CameraRig.Target = LocalPlayer;
			EnemySpawner.SetTarget(LocalPlayer);

			AttachHUD();
			LocalPlayer.ConfigureObjectPlacement(WorldObjectManager!, this, HUD!.GetNode<Hotbar>("Hotbar"));
		}

		private void AttachHUD() {
			HUD = HUDScene.Instantiate<HUD>();
			SubscribeToEvents(HUD);
			HUD.Init(LocalPlayer!, StateMachine);
			LocalPlayer!.UseItemComponent.UserHotbar = HUD.GetNode<Hotbar>("Hotbar");

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

			UnsubscribeFromPlayerItem3DIconEvents(LocalPlayer);
			LocalPlayer.QueueFree();
			LocalPlayer = null;

			SpawnLocalPlayer();

			LocalPlayer!.Inventory.Import(inventoryData);
			LocalPlayer.Hotbar.Import(hotbarData);

			Log.Info("Player respawned");
		}

		public bool SaveGame(string fileName) {
			if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig)) {
				Log.Error("Cannot save game: game objects are not valid");
				return false;
			}

			var data = new GameState {
				Player = LocalPlayer.Export(),
				CameraRig = CameraRig.Export(),
				Item3DIconManager = Item3DIconManager!.Export(),
				WorldObjectManager = WorldObjectManager!.Export(),
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
			if(LoadFile != null) {
				LoadData(LoadFile);
				LoadFile = null;
				return;
			}
			SpawnTestItems();
		}

		private void LoadData(string file) {
			var data = SaveService.Load<GameState>(file);

			LocalPlayer!.Import(data.Player);
			CameraRig!.Import(data.CameraRig);
			Item3DIconManager!.Import(data.Item3DIconManager);
			WorldObjectManager!.Import(data.WorldObjectManager);
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
				UnsubscribeFromPlayerItem3DIconEvents(LocalPlayer!);
			}

			Cleanup(LocalPlayer);
			LocalPlayer = null;

			Cleanup(Item3DIconManager);
			Item3DIconManager = null;

			EnemySpawner.Reset();
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

		private void SpawnTestItems() {
			if(Item3DIconManager == null) {
				Log.Error("Cannot spawn test items: Item3DIconManager is not initialized");
				return;
			}
			Item3DIconManager.SpawnItem(ItemID.AppleRed, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.AppleYellow, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.AppleGreen, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.BananaYellow, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.BananaGreen, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.StrawberryGreen, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.StrawberryRed, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.StrawberryRed, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.StrawberryRed, RandomLocationNearPlayer());
			Item3DIconManager.SpawnItem(ItemID.StrawberryRed, RandomLocationNearPlayer());

			Item3DIconManager.SpawnItem(ItemID.StrawberryRed, RandomLocationNearPlayer(), 3);
		}

		private void SubscribeToPlayerItem3DIconEvents(Player player) {
			if(Item3DIconManager == null) {
				return;
			}
			player.InventoryManager.SpawnItem3DIconRequested += (item, position) => Item3DIconManager.RequestSpawnItem(item, position);
			player.PickupComponent.DespawnItem3DIconRequested += Item3DIconManager.RequestDespawnItem;
		}

		private void UnsubscribeFromPlayerItem3DIconEvents(Player player) {
			if(Item3DIconManager == null) {
				return;
			}
			player.InventoryManager.SpawnItem3DIconRequested -= (item, position) => Item3DIconManager.RequestSpawnItem(item, position);
			player.PickupComponent.DespawnItem3DIconRequested -= Item3DIconManager.RequestDespawnItem;
		}
	}

	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
		public Item3DIconManagerData Item3DIconManager { get; init; }
		public WorldObjectManagerData WorldObjectManager { get; init; }
	}
}
