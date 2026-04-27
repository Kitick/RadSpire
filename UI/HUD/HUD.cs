namespace UI.HUD;

using System;
using Character;
using Crafting.Interface;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using Network.Panels;
using PauseMenu;
using QuestSystem;
using RespawnMenu;
using Root;
using SaveMenu;
using Services;
using Settings.Interface;
using UI;
using MenuState = GameWorld.GameManager.MenuState;

public sealed partial class HUD : Control {
	private static readonly LogService Log = new(nameof(HUD), enabled: true);

	[ExportCategory("HUD Elements")]
	[Export] private Button PauseButton = null!;
	[Export] private PauseMenu PauseMenu = null!;
	[Export] private InventoryUI Inventory = null!;
	[Export] private CraftingUI CraftingUI = null!;
	[Export] public InventoryItemInformationUI InventoryItemInformationUI = null!;
	[Export] private InventoryUI Chest = null!;
	[Export] private InventoryUI BuildUI = null!;
	[Export] private QuestLog QuestLog = null!;
	[Export] public Hotbar Hotbar = null!;
	[Export] private RespawnMenu RespawnMenu = null!;
	[Export] private SegmentBar HealthBar = null!;
	[Export] private Label HealthLabel = null!;
	[Export] private Control WinMenu = null!;

	[ExportCategory("HUD Scenes")]
	[Export] private PackedScene SettingsScene = null!;
	[Export] private PackedScene SaveMenuScene = null!;
	[Export] private PackedScene HostPanelScene = null!;

	[ExportCategory("Health Colors")]
	[Export] private Color HealthColor = new(0.2f, 0.6f, 0.2f);
	[Export] private Color RadiationColor = new(0.6f, 0.2f, 0.8f);
	[Export] private Color EmptyColor = new(0.2f, 0.2f, 0.2f);

	public Player Player = null!;

	private StateMachine<MenuState> StateMachineRef = null!;
	private event Action? OnExit;
	private Label InteractionPrompt = null!;

	public event Action? ResumeRequested;
	public event Action? PauseRequested;
	public event Action? SettingsRequested;
	public event Action? HostRequested;
	public event Action? MainMenuRequested;
	public event Action? RespawnRequested;
	public event Action<bool>? InventoryRequested;
	public event Action<bool>? ChestRequested;
	public event Action<bool>? BuildUIRequested;
	public event Action<string>? SaveRequested;

	public void Init(Player player, StateMachine<MenuState> stateMachine, QuestManager questManager) {
		Player = player;
		Player.Health.OnChanged += (_, _) => UpdateHealthBar();
		Player.Radiation.OnChanged += (_, _) => UpdateHealthBar();
		CraftingUI.Inventories.Add(player.Inventory);
		CraftingUI.Inventories.Add(player.Hotbar);
		StateMachineRef = stateMachine;
		Inventory.Initialize(player.Inventory, player);
		Hotbar.Initialize(player.Hotbar, player);
		BuildUI.Visible = false;
		InventoryItemInformationUI.SetUpInventoryItemInformationUI();
		ConfigureStateMachine(stateMachine);
		QuestLog?.Init(questManager);
		UpdateHealthBar();
	}

	public override void _Ready() {
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.Always;

		InteractionPrompt = GetNode<Label>("InteractionPrompt");
		InteractionPrompt.Visible = false;

		SetCallbacks();
		SetInputCallbacks();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		ClearEvents();
	}

	private void ClearEvents() {
		ResumeRequested = null;
		PauseRequested = null;
		SettingsRequested = null;
		HostRequested = null;
		MainMenuRequested = null;
		RespawnRequested = null;
		InventoryRequested = null;
		ChestRequested = null;
		BuildUIRequested = null;
		SaveRequested = null;
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.Inventory.WhenPressed(ToggleInventory);

		OnExit += ActionEvent.MenuExit.WhenPressed(() => {
			if(StateMachineRef.CurrentState == MenuState.Game) { PauseRequested?.Invoke(); } else if(StateMachineRef.CurrentState != MenuState.Game) { ResumeRequested?.Invoke(); }
		});

		OnExit += ActionEvent.QuestLog.WhenPressed(ToggleQuestLog);
	}

	private void ToggleQuestLog() {
		QuestLog?.Visible = !QuestLog.Visible;
	}

	private void ConfigureStateMachine(StateMachine<MenuState> stateMachine) {
		// Game state - normal gameplay
		stateMachine.OnEnter(MenuState.Game, () => {
			PauseMenu.CloseMenu();
			RespawnMenu.CloseMenu();
			Inventory.Visible = false;
			CraftingUI.Visible = false;
			BuildUI.Visible = false;
			BuildUIRequested?.Invoke(false);
			InventoryItemInformationUI.Visible = false;
		});

		// Paused state
		stateMachine.OnEnter(MenuState.Paused, PauseMenu.OpenMenu);
		stateMachine.OnExit(MenuState.Paused, PauseMenu.CloseMenu);

		// Settings state
		stateMachine.OnEnter(MenuState.Settings, OpenSettingsPanel);

		// Inventory state
		stateMachine.OnEnter(MenuState.Inventory, () => {
			Inventory.Visible = true;
			CraftingUI.Visible = true;
			Hotbar.Visible = true;
			CraftingUI.RefreshUI();
			InventoryItemInformationUI.Visible = true;
			InventoryRequested?.Invoke(true);
		});

		stateMachine.OnExit(MenuState.Inventory, () => {
			Inventory.Visible = false;
			CraftingUI.Visible = false;
			InventoryItemInformationUI.Visible = false;
			InventoryRequested?.Invoke(false);
		});

		// Chest state
		stateMachine.OnEnter(MenuState.Chest, () => {
			Chest.Visible = true;
			ChestRequested?.Invoke(true);
			Inventory.Visible = true;
			InventoryItemInformationUI.Visible = true;
			Hotbar.Visible = true;
			InventoryRequested?.Invoke(true);
		});

		stateMachine.OnExit(MenuState.Chest, () => {
			Chest.Visible = false;
			ChestRequested?.Invoke(false);
			Inventory.Visible = false;
			InventoryItemInformationUI.Visible = false;
			Hotbar.Visible = true;
			InventoryRequested?.Invoke(false);
		});

		// Build state
		stateMachine.OnEnter(MenuState.Build, () => {
			BuildUI.Visible = true;
			Hotbar.Visible = true;
			BuildUIRequested?.Invoke(true);
		});
		stateMachine.OnExit(MenuState.Build, () => {
			BuildUI.Visible = false;
			BuildUIRequested?.Invoke(false);
		});

		// Host state
		stateMachine.OnEnter(MenuState.Host, OpenHostPanel);

		// Death state
		stateMachine.OnEnter(MenuState.Death, RespawnMenu.OpenMenu);
		stateMachine.OnExit(MenuState.Death, RespawnMenu.CloseMenu);
	}

	private void SetCallbacks() {
		// Pause button in HUD
		PauseButton.Pressed += () => PauseRequested?.Invoke();

		// Pause menu buttons
		PauseMenu.ResumeButton.Pressed += () => ResumeRequested?.Invoke();
		PauseMenu.SaveButton.Pressed += OpenSaveMenu;
		PauseMenu.HostButton.Pressed += () => HostRequested?.Invoke();
		PauseMenu.SettingsButton.Pressed += () => SettingsRequested?.Invoke();
		PauseMenu.MainMenuButton.Pressed += () => MainMenuRequested?.Invoke();

		// Respawn menu buttons
		RespawnMenu.RespawnButton.Pressed += () => RespawnRequested?.Invoke();
		RespawnMenu.MainMenuButton.Pressed += () => MainMenuRequested?.Invoke();

	}

	private void UpdateHealthBar() {
		int current = Player.Health.Current;
		int effectiveMax = Player.Health.Max;
		int baseMax = Player.BaseMaxHealth;
		int radiated = baseMax - effectiveMax;

		HealthBar.SetSegments(
			new(current, HealthColor),
			new(effectiveMax - current, EmptyColor),
			new(radiated, RadiationColor)
		);

		HealthLabel.Text = $"{current} / {effectiveMax}";
	}

	public void Win() {
		WinMenu.Visible = true;
		GetTree().CreateTimer(5.0f).Timeout += () => { GetTree().Quit(); };
	}

	private void OpenSettingsPanel() {
		SettingsMenu settings = this.AddScene<SettingsMenu>(SettingsScene);
		settings.TreeExited += () => PauseRequested?.Invoke();
		settings.OpenMenu();
	}

	private void OpenHostPanel() {
		HostPanel hostPanel = this.AddScene<HostPanel>(HostPanelScene);
		hostPanel.UpdateHostText("Host Game");
		hostPanel.OpenMenu();
	}

	private void OpenSaveMenu() {
		SaveMenu saveMenu = this.AddScene<SaveMenu>(SaveMenuScene);
		saveMenu.OnSave += fileName => SaveRequested?.Invoke(fileName);
		saveMenu.OpenMenu(SaveMenu.SaveMode.Save);
	}

	public void ShowInteractionPrompt(string text) {
		InteractionPrompt.Text = text;
		InteractionPrompt.Visible = true;
	}

	public void HideInteractionPrompt() {
		InteractionPrompt.Visible = false;
	}

	public void ShowQuestNotification(string text) {
		ShowInteractionPrompt(text);
	}

	private void ToggleInventory() {
		if(!StateMachineRef.IsSettled) {
			Log.Info("state machine not started, starting at Game");
			StateMachineRef.Start(MenuState.Game);
		}

		if(StateMachineRef.CurrentState == MenuState.Chest) {
			Log.Info("Closing Chest");
			StateMachineRef.TransitionTo(MenuState.Game);
		}
		else if(StateMachineRef.CurrentState == MenuState.Inventory) {
			Log.Info("Closing Inventory");
			StateMachineRef.TransitionTo(MenuState.Game);
		}
		else {
			Log.Info("Opening Inventory");
			StateMachineRef.TransitionTo(MenuState.Inventory);
		}
	}

	public void ToggleChest() {
		if(!StateMachineRef.IsSettled) {
			Log.Info("state machine not started, starting at Game");
			StateMachineRef.Start(MenuState.Game);
		}

		if(StateMachineRef.CurrentState == MenuState.Chest) {
			Log.Info("Closing Chest");
			StateMachineRef.TransitionTo(MenuState.Game);
		}
		else {
			Log.Info("Opening Chest");
			StateMachineRef.TransitionTo(MenuState.Chest);
		}
	}

	public void OpenChest(Inventory chestInventory, Player player) {
		if(chestInventory == null || player == null) {
			Log.Error("chestInventory or player is null");
			return;
		}

		chestInventory.Name = "Chest";

		Chest.Initialize(chestInventory, player);
		Chest.SetLabelText("Chest");

		if(!StateMachineRef.IsSettled) {
			StateMachineRef.Start(MenuState.Game);
		}

		if(StateMachineRef.CurrentState != MenuState.Chest) {
			ToggleChest();
		}
	}

	public InventoryUI GetBuildUI() => BuildUI;

	public void OpenBuildUI() {
		if(!StateMachineRef.IsSettled) {
			StateMachineRef.Start(MenuState.Game);
		}
		if(StateMachineRef.CurrentState != MenuState.Build) {
			StateMachineRef.TransitionTo(MenuState.Build);
		}
	}

	public void CloseBuildUI() {
		if(!StateMachineRef.IsSettled) {
			return;
		}
		if(StateMachineRef.CurrentState == MenuState.Build) {
			StateMachineRef.TransitionTo(MenuState.Game);
		}
	}
}
