using System;
using Core;
using Godot;
using InputSystem;
using MultiplayerPanels;
using Network;
using Settings;

public sealed partial class HUD : Control {
	private static readonly Logger Log = new(nameof(HUD), enabled: true);

	public enum MenuState { Game, Paused, Settings, Inventory, Host, Death };

	private readonly StateMachine<MenuState> StateMachine = new();
	public MenuState State => StateMachine.CurrentState;

	public Player Player = null!;

	[ExportCategory("HUD Elements")]
	[Export] private Button PauseButton = null!;
	[Export] private PauseMenu PauseMenu = null!;
	[Export] private InventoryUI Inventory = null!;
	[Export] private Control QuestLog = null!;
	[Export] private Hotbar Hotbar = null!;
	[Export] private RespawnMenu RespawnMenu = null!;
	[Export] private ProgressBar HealthBar = null!;

	[ExportCategory("HUD Scenes")]
	[Export] private PackedScene SettingsScene = null!;
	[Export] private PackedScene SaveMenu = null!;
	[Export] private PackedScene HostPanelScene = null!;

	public bool IsPaused => GetTree().Paused;

	private event Action? OnExit;

	public override void _EnterTree() {
		base._EnterTree();
		Player = GetParent<Player>();
		if(Player == null) {
			Log.Error("HUD could not find Player node in parent.");
		}
		else {
			Log.Info("HUD successfully found Player node in parent.");
		}
	}

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;

		if(Player == null) {
			Log.Error("No Player node defined.");
			return;
		}

		SetInputCallbacks();
		SetCallbacks();
		SubscribeToPlayerHealth();

		StateMachine.TransitionTo(MenuState.Game);
	}

	private void SubscribeToPlayerHealth() {
		HealthBar.MaxValue = Player.Health.MaxHealth;
		HealthBar.Value = Player.Health.CurrentHealth;
		Player.Health.OnHealthChanged += (from, to) => HealthBar.Value = to;
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		Server.Instance.OnServerDisconnected -= OnServerDisconnected;
	}

	private void OnServerDisconnected() {
		Log.Info("Server disconnected, returning to main menu...");
		QuitGame();
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuExit.WhenPressed(TogglePause);
		OnExit += ActionEvent.Inventory.WhenPressed(ToggleInventory);
	}

	private void SetCallbacks() {
		PauseButton.Pressed += TogglePause;

		PauseMenu.ResumeButton.Pressed += TogglePause;
		PauseMenu.SaveButton.Pressed += OpenSaveMenu;
		PauseMenu.HostButton.Pressed += OnHostButtonPressed;
		PauseMenu.SettingsButton.Pressed += () => StateMachine.TransitionTo(MenuState.Settings);
		PauseMenu.MainMenuButton.Pressed += QuitGame;

		RespawnMenu.RespawnButton.Pressed += Respawn;
		RespawnMenu.MainMenuButton.Pressed += QuitGame;

		Server.Instance.OnServerDisconnected += OnServerDisconnected;
	}

	private void OnHostButtonPressed() {
		if(Server.Instance.IsNetworkConnected) {
			Log.Info("Disconnecting from network...");
			Server.Instance.Disconnect();
			PauseMenu.UpdateHostButtonText();
		}
		else {
			Log.Info("Opening host panel...");
			StateMachine.TransitionTo(MenuState.Host);
		}
	}

	private void Respawn() {
		GetTree().Paused = false;

		Player = GameManager.Instance.RespawnPlayer();

		StateMachine.TransitionTo(MenuState.Game);
	}

	public void ShowRespawnMenu() {
		StateMachine.TransitionTo(MenuState.Death);
	}

	private void OnStateChanged(MenuState from, MenuState to) {
		GetTree().Paused = to != MenuState.Game && to != MenuState.Death;

		if(to == MenuState.Paused) { PauseMenu.OpenMenu(); }
		else { PauseMenu.CloseMenu(); }

		if(to == MenuState.Death) { RespawnMenu.OpenMenu(); }
		else { RespawnMenu.CloseMenu(); }

		if(to == MenuState.Host) { OpenHostPanel(); }
		if(to == MenuState.Settings) { OpenSettings(); }
	}

	private void OpenHostPanel() {
		var hostpanel = HostPanelScene.Instantiate<HostPanel>();
		hostpanel.UpdateHostText("Host Game");
		hostpanel.OpenMenu(onClose: () => StateMachine.TransitionTo(MenuState.Paused));
	}

	private void OpenSettings() {
		var settings = SettingsScene.Instantiate<SettingsMenu>();
		settings.OpenMenu(
			onClose: () => StateMachine.TransitionTo(MenuState.Paused)
		);
	}

	private void OpenSaveMenu() {
		var savemenu = SaveMenu.Instantiate<SaveMenu>();
		savemenu.Mode = SaveMenuMode.Save;
		savemenu.OpenMenu(onClose: () => StateMachine.TransitionTo(MenuState.Paused));
	}

	private void TogglePause() {
		if(State == MenuState.Death) { return; }
		StateMachine.TransitionTo(IsPaused ? MenuState.Game : MenuState.Paused);
	}

	public static void QuitGame() {
		GameManager.Instance.ReturnToMainMenu();
	}

	public void ToggleInventory() {
		// Don't allow inventory while dead
		if(State == MenuState.Death) { return; }
		if(!Inventory.Visible) {
			Inventory.Visible = true;
			Hotbar.Visible = true;
			StateMachine.TransitionTo(MenuState.Inventory);
		}
		else {
			Inventory.Visible = false;
			StateMachine.TransitionTo(MenuState.Game);
		}
	}
}
