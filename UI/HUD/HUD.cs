using System;
using Core;
using Godot;
using Settings;

public sealed partial class HUD : Control {
	public enum MenuState { Game, Paused, Settings };
	public MenuState State = MenuState.Game;

	private PauseMenu PauseMenu = null!;
	private Control Inventory = null!;
	private Control QuestLog = null!;
	private Hotbar Hotbar = null!;
	private SettingsMenu Settings = null!;

	private const string PAUSE_BUTTON = "PauseButton";
	private const string PAUSE_MENU = "PauseMenu";
	private const string INVENTORY = "Inventory";
	private const string QUESTLOG = "QuestLog";
	private const string HOTBAR = "Hotbar";
	private const string SETTINGS = "Settings";

	public bool IsPaused => PauseMenu.Visible;

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;

		GetComponents();
		SetCallbacks();
	}

	public override void _Input(InputEvent input) {
		if(input.IsActionPressed(Actions.MenuExit)) {
			TogglePause();
		}
	}

	private void GetComponents() {
		PauseMenu = GetNode<PauseMenu>(PAUSE_MENU);
		Settings = GetNode<SettingsMenu>(SETTINGS);
		Inventory = GetNode<Control>(INVENTORY);
		QuestLog = GetNode<Control>(QUESTLOG);
		Hotbar = GetNode<Hotbar>(HOTBAR);
	}

	private void SetCallbacks() {
		GetNode<Button>(PAUSE_BUTTON).Pressed += TogglePause;

		PauseMenu.ResumeButton.Pressed += () => TogglePause(false);
		PauseMenu.SaveButton.Pressed += SaveGame;

		PauseMenu.SettingsButton.Pressed += () => ToggleSettings(true);
		PauseMenu.MainMenuButton.Pressed += QuitGame;
	}

	public void TogglePause() => TogglePause(!IsPaused);

	public void TogglePause(bool state) {
		GetTree().Paused = state;
		PauseMenu.Visible = state;
	}

	public void ToggleSettings() => ToggleSettings(!Settings.Visible);

	public void ToggleSettings(bool state) {
		Settings.Visible = state;
		PauseMenu.Visible = state;
	}

	public void SaveGame() {
		GameManager.Save("autosave");
	}

	public void QuitGame() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}
}
