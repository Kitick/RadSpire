using System;
using Core;
using Godot;
using InputSystem;
using MultiplayerPanels;
using Network;
using Settings;

public sealed partial class HUD : Control {
	private static readonly Logger Log = new(nameof(HUD), enabled: true);

	public enum MenuState { Game, Paused, Settings, Inventory, Host };

	private readonly FiniteStateMachine<MenuState> StateMachine;
	public MenuState State => StateMachine.State;

	private PauseMenu PauseMenu = null!;
	private InventoryUI Inventory = null!;
	private Control QuestLog = null!;
	private Hotbar Hotbar = null!;

	private const string PAUSE_BUTTON = "PauseButton";
	private const string PAUSE_MENU = "PauseMenu";
	private const string INVENTORY = "Inventory";
	private const string QUESTLOG = "QuestLog";
	private const string HOTBAR = "Hotbar";

	//Load Scene Reference

	private HostPanel HostPanel = null!;

	public bool IsPaused => GetTree().Paused;

	private event Action? OnExit;
	public Player Player = null!;
	public bool InventoryOpen => Inventory.Visible;

	public HUD() {
		StateMachine = new(MenuState.Game, OnStateChanged);
	}

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

		LoadScenes();
		GetComponents();
		SetInputCallbacks();
		SetCallbacks();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		Server.Instance.OnServerDisconnected -= OnServerDisconnected;
	}

	private void OnServerDisconnected() {
		Log.Info("Server disconnected, returning to main menu...");
		QuitGame();
	}

	public void LoadScenes() {
		var packed1 = GD.Load<PackedScene>("res://UI/MultiplayerPanels/HostPanel/HostPanel.tscn");
		HostPanel = packed1.Instantiate<HostPanel>();
		HostPanel.Visible = false;
		AddChild(HostPanel);
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuExit.WhenPressed(TogglePause);
		OnExit += ActionEvent.Inventory.WhenPressed(ToggleInventory);
	}

	private void GetComponents() {
		PauseMenu = GetNode<PauseMenu>(PAUSE_MENU);
		Inventory = GetNode<InventoryUI>(INVENTORY);
		QuestLog = GetNode<Control>(QUESTLOG);
		Hotbar = GetNode<Hotbar>(HOTBAR);
	}

	private void SetCallbacks() {
		GetNode<Button>(PAUSE_BUTTON).Pressed += () => StateMachine.TransitionTo(MenuState.Paused);

		PauseMenu.ResumeButton.Pressed += TogglePause;
		PauseMenu.SaveButton.Pressed += OpenSaveMenu;
		PauseMenu.HostButton.Pressed += OnHostButtonPressed;
		PauseMenu.SettingsButton.Pressed += () => StateMachine.TransitionTo(MenuState.Settings);
		PauseMenu.MainMenuButton.Pressed += QuitGame;

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

	private void OnStateChanged(MenuState from, MenuState to) {
		GetTree().Paused = to != MenuState.Game;

		if(to == MenuState.Paused) {
			PauseMenu.OpenMenu();
		}
		else {
			PauseMenu.CloseMenu();
		}

		if(to == MenuState.Host) { OpenHostPanel(); }
		if(to == MenuState.Settings) { OpenSettings(); }
	}

	private void OpenHostPanel() {
		var host = this.AddScene<HostPanel>(Scenes.HostPanel);
		host.OnMenuClosed += () => StateMachine.TransitionTo(MenuState.Paused);
		host.OpenMenu();

		host.UpdateHostText("Host Game");
	}

	private void OpenSettings() {
		var settings = this.AddScene<SettingsMenu>(Scenes.SettingsMenu);
		settings.OnMenuClosed += () => StateMachine.TransitionTo(MenuState.Paused);
		settings.OpenMenu();
	}

	private void OpenSaveMenu() {
		var saveLoad = this.AddScene<SaveLoadMenu>(Scenes.SaveLoadMenu);
		saveLoad.Mode = SaveLoadMode.Save;
		saveLoad.OnMenuClosed += () => StateMachine.TransitionTo(MenuState.Paused);
		saveLoad.OpenMenu();
	}

	private void TogglePause() {
		StateMachine.TransitionTo(IsPaused ? MenuState.Game : MenuState.Paused);
	}

	public void QuitGame() {
		GameManager.Instance.ReturnToMainMenu();
	}

	public void ToggleInventory() {
		if(!InventoryOpen) {
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
