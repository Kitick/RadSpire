namespace Root {
	using System;
	using Camera;
	using Character;
	using Components;
	using Core;
	using Godot;
	using ItemSystem;
	using Services;
	using UI;

	public sealed partial class GameManager : Node {
		private static readonly LogService Log = new(nameof(GameManager), enabled: true);

		[ExportCategory("Scene References")]
		[Export] private PackedScene CameraScene = null!;
		[Export] private PackedScene HUDScene = null!;
		[Export] private PackedScene PlayerScene = null!;
		[Export] private PackedScene EnemyScene = null!;
		[Export] private PackedScene Item3DIconManagerScene = null!;

		private readonly KeyInput KeyInput = new();
		private CameraRig CameraRig = null!;
		private Player? LocalPlayer;
		private HUD? HUD;
		private Item3DIconManager? Item3DIconManager;
		public Action? MainMenuRequested;

		public enum MenuState { Game, Paused, Settings, Inventory, Host, Death }
		private readonly StateMachine<MenuState> StateMachine = new(MenuState.Game);

		private string? LoadFile;

		private const int SpawnHeight = 4;
		private const int SpawnRadius = 50;

		private static readonly Vector3 PlayerSpawnLocation = new Vector3(0, SpawnHeight, 0);

		private float SpawnTimer = 5.0f;
		private int EnemyCount;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			CameraRig = this.AddScene<CameraRig>(CameraScene);
			Item3DIconManager = this.AddScene<Item3DIconManager>(Item3DIconManagerScene);
			ConfigureStateMachine();

			StartGame();
		}

		public override void _PhysicsProcess(double delta) {
			if(!IsInstanceValid(LocalPlayer)) { return; }

			float dt = (float) delta;

			KeyInput.Update(CameraRig);
			LocalPlayer.Update(dt, KeyInput);

			UpdateTimer();
		}

		private void ConfigureStateMachine() {
			StateMachine.OnEnter(MenuState.Game, () => GetTree().Paused = false);
			StateMachine.OnExit(MenuState.Game, () => GetTree().Paused = true);
		}

		private void SpawnLocalPlayer() {
			LocalPlayer = this.AddScene<Player>(PlayerScene);
			LocalPlayer.GlobalPosition = PlayerSpawnLocation;

			CameraRig.Target = LocalPlayer;

			AttachHUD();
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

			LocalPlayer.QueueFree();
			LocalPlayer = null;

			SpawnLocalPlayer();

			LocalPlayer!.Inventory.Import(inventoryData);
			LocalPlayer.Hotbar.Import(hotbarData);

			Log.Info("Player respawned");
		}

		private void UpdateTimer() {
			SpawnTimer -= 0.015f;

			if(SpawnTimer <= 0.0f && EnemyCount < 5) {
				GD.Print("Spawned");
				SpawnTimer = (float) GD.RandRange(1f, 6f);
				var enemy = this.AddScene<Enemy>(EnemyScene);
				enemy.GlobalPosition = GetRandomEnemySpawn();
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

		public bool SaveGame(string fileName) {
			if(!IsInstanceValid(LocalPlayer) || !IsInstanceValid(CameraRig)) {
				Log.Error("Cannot save game: game objects are not valid");
				return false;
			}

			var data = new GameState {
				Player = LocalPlayer.Export(),
				CameraRig = CameraRig.Export(),
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
			SpawnTestItems();

			if(LoadFile != null) {
				LoadData(LoadFile);
				LoadFile = null;
			}
		}

		private void LoadData(string file) {
			var data = SaveService.Load<GameState>(file);

			LocalPlayer!.Import(data.Player);
			CameraRig!.Import(data.CameraRig);
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

			// Reset spawn state
			EnemyCount = 0;
			SpawnTimer = 5.0f;
		}

		private void SpawnTestItem(string itemID, Vector3 position, float scaleFactor = 1.0f) {
			Item? item = ItemDataBaseManager.Instance.CreateItemInstanceById(itemID);
			if(item == null) {
				Log.Error($"Failed to load item with ID: {itemID}");
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
			SpawnTestItem(ItemID.AppleRed, RandomLocation());
			SpawnTestItem(ItemID.AppleYellow, RandomLocation());
			SpawnTestItem(ItemID.AppleGreen, RandomLocation());
			SpawnTestItem(ItemID.BananaYellow, RandomLocation());
			SpawnTestItem(ItemID.BananaGreen, RandomLocation());
			SpawnTestItem(ItemID.StrawberryGreen, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, RandomLocation());
			SpawnTestItem(ItemID.StrawberryRed, new Vector3(40, SpawnHeight, 20), 3);
		}
	}

	public readonly struct GameState : ISaveData {
		public PlayerData Player { get; init; }
		public CameraRigData CameraRig { get; init; }
	}
}
