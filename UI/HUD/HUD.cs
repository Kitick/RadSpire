using System;
using Core;
using Godot;
using InputSystem;
using Settings;

public sealed partial class HUD : Control {
	public enum MenuState { Game, Paused, Settings };
	public MenuState State {
		get;
		set {
			ChangeState(field, value);
			field = value;
		}
	} = MenuState.Game;

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

	public override void _EnterTree() {
		ProcessMode = ProcessModeEnum.Always;

		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
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
		GetNode<Button>(PAUSE_BUTTON).Pressed += () => State = MenuState.Paused;

		PauseMenu.ResumeButton.Pressed += TogglePause;
		PauseMenu.SaveButton.Pressed += SaveGame;
		PauseMenu.SettingsButton.Pressed += () => State = MenuState.Settings;
		PauseMenu.MainMenuButton.Pressed += QuitGame;
	}

	private void ChangeState(MenuState from, MenuState to) {
		GetTree().Paused = to != MenuState.Game;
		PauseMenu.Visible = to == MenuState.Paused;

		if(to == MenuState.Settings) { OpenSettings(); }
	}

	private void OpenSettings() {
		var settings = this.AddScene<SettingsMenu>(Scenes.SettingsMenu);
		settings.OnMenuClosed += () => State = MenuState.Paused;
		settings.OpenMenu();
	}

	private void TogglePause() {
		State = IsPaused ? MenuState.Game : MenuState.Paused;
	}

	public static void SaveGame() {
		GameManager.Save("autosave");
	}

	public void QuitGame() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}
}
