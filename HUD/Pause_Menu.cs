using System;
using Core;
using Godot;

public partial class Pause_Menu : Control {
	private Button ResumeButton = null!;
	private Button SettingsButton = null!;
	private Button SaveButton = null!;
	private Button MainMenuButton = null!;
	[Export] public PackedScene SettingsMenu = null!;
	private Control SettingsInstance = null!;

	private const string RESUME_BUTTON = "Pause_Buttons/Resume";
	private const string SETTINGS_BUTTON = "Pause_Buttons/Settings";
	private const string SAVE_BUTTON = "Pause_Buttons/Save";
	private const string MAIN_MENU_BUTTON = "Pause_Buttons/Main_Menu";

	public override void _Ready() {
		SettingsMenu = GD.Load<PackedScene>(Scenes.SettingsMenu);
		Visible = false;
		ProcessMode = ProcessModeEnum.WhenPaused;
		GetComponents();
		SetCallBacks();
	}

	public override void _UnhandledInput(InputEvent @event) {
		// Only close on Esc if the menu is currently shown
		if(Visible && @event.IsActionPressed(Actions.UICancel)) {
			OnResumeButtonPressed();
			GetViewport().SetInputAsHandled(); // stop further Esc handling this frame
		}
	}

	private void GetComponents() {
		ResumeButton = GetNode<Button>(RESUME_BUTTON);
		SettingsButton = GetNode<Button>(SETTINGS_BUTTON);
		SaveButton = GetNode<Button>(SAVE_BUTTON);
		MainMenuButton = GetNode<Button>(MAIN_MENU_BUTTON);
	}

	private void SetCallBacks() {
		ResumeButton.Pressed += OnResumeButtonPressed;
		SettingsButton.Pressed += OnSettingsButtonPressed;
		SaveButton.Pressed += OnSaveButtonPressed;
		MainMenuButton.Pressed += OnMainMenuButtonPressed;
	}

	private void OnResumeButtonPressed() {
		GetTree().Paused = false;
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnSaveButtonPressed() {
		GameManager.Save("autosave");
	}

	private void OnMainMenuButtonPressed() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(Scenes.MainMenu);
	}

	private void OnSettingsButtonPressed() {
		// Only one settings overlay at a time
		if(SettingsInstance == null || !SettingsInstance.IsInsideTree()) {
			SettingsInstance = SettingsMenu.Instantiate<Control>();
			AddChild(SettingsInstance);

			// Let the settings menu decide its own ProcessMode (it sets Always in _Ready)
			// So DO NOT change SettingsInstance.ProcessMode here.
		}

		// Just ensure it's visible when button is pressed

		SettingsInstance.Visible = true;
	}
}
