using System;
using Core;
using Godot;
using InputSystem;
using Settings;
using Host;

public sealed partial class HUD : Control {
	public enum MenuState { Game, Paused, Settings, Host };

	private readonly FiniteStateMachine<MenuState> StateMachine;
	public MenuState State => StateMachine.State;

	private PauseMenu PauseMenu = null!;
	private Control Inventory = null!;
	private Control QuestLog = null!;
	private Hotbar Hotbar = null!;

	private const string PAUSE_BUTTON = "PauseButton";
	private const string PAUSE_MENU = "PauseMenu";
	private const string INVENTORY = "Inventory";
	private const string QUESTLOG = "QuestLog";
	private const string HOTBAR = "Hotbar";

	public bool IsPaused => GetTree().Paused;

	private event Action? OnExit;

	public HUD() {
		StateMachine = new(MenuState.Game, OnStateChanged);
	}

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;

		SetInputCallbacks();
		GetComponents();
		SetCallbacks();
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	private void SetInputCallbacks() {
		OnExit += ActionEvent.MenuExit.WhenPressed(TogglePause);
	}

	private void GetComponents() {
		PauseMenu = GetNode<PauseMenu>(PAUSE_MENU);
		Inventory = GetNode<Control>(INVENTORY);
		QuestLog = GetNode<Control>(QUESTLOG);
		Hotbar = GetNode<Hotbar>(HOTBAR);
	}

	private void SetCallbacks() {
		GetNode<Button>(PAUSE_BUTTON).Pressed += () => StateMachine.TransitionTo(MenuState.Paused);

		PauseMenu.ResumeButton.Pressed += TogglePause;
		PauseMenu.SaveButton.Pressed += SaveGame;
		PauseMenu.HostButton.Pressed += () => StateMachine.TransitionTo(MenuState.Host);
		PauseMenu.SettingsButton.Pressed += () => StateMachine.TransitionTo(MenuState.Settings);
		PauseMenu.MainMenuButton.Pressed += QuitGame;
	}

	private void OnStateChanged(MenuState from, MenuState to) {
		GetTree().Paused = to != MenuState.Game;
		PauseMenu.Visible = to == MenuState.Paused;
		
		if(to == MenuState.Host) { OpenHostPanel(); }
		if(to == MenuState.Settings) { OpenSettings(); }
	}

	private void OpenHostPanel() {
		var host = this.AddScene<HostPanel>(Scenes.HostPanel);
		host.OnMenuClosed += () => StateMachine.TransitionTo(MenuState.Paused);
		host.OpenMenu();
	}

	private void OpenSettings() {
		var settings = this.AddScene<SettingsMenu>(Scenes.SettingsMenu);
		settings.OnMenuClosed += () => StateMachine.TransitionTo(MenuState.Paused);
		settings.OpenMenu();
	}

	private void TogglePause() {
		StateMachine.TransitionTo(IsPaused ? MenuState.Game : MenuState.Paused);
	}

	public static void SaveGame() {
		GameManager.Save("autosave");
	}

	public void QuitGame() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}
}