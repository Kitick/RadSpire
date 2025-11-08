using Godot;
using System;

public partial class Pause_Menu : Control {

	private Button ResumeButton = null!;
	private Button SettingsButton = null!;
	private Button SaveButton = null!;
	private Button MainMenuButton = null!;
	[Export] public PackedScene SettingsMenu = null!;
	private Control SettingsInstance = null!;

	public override void _Ready() {
		SettingsMenu = GD.Load<PackedScene>("res://Settings Menu/Settings_Menu.tscn");
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		GetComponents();
		SetCallBacks();
	}

	public override void _UnhandledInput(InputEvent @event) {
		// Only close on Esc if the menu is currently shown
		if(Visible && @event.IsActionPressed("ui_cancel")) {
			OnResumeButtonPressed();
			GetViewport().SetInputAsHandled(); // stop further Esc handling this frame
		}
	}

	private void GetComponents() {

		ResumeButton = GetNode<Button>("Pause_Buttons/Resume");
		SettingsButton = GetNode<Button>("Pause_Buttons/Settings");
		SaveButton = GetNode<Button>("Pause_Buttons/Save");
		MainMenuButton = GetNode<Button>("Pause_Buttons/Main_Menu");
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
		GetTree().ChangeSceneToFile("res://Main Menu/Main_Menu.tscn");
	}

	private void OnSettingsButtonPressed()
	{
		// Only one settings overlay at a time
		if (SettingsInstance == null || !SettingsInstance.IsInsideTree())
		{
			SettingsInstance = SettingsMenu.Instantiate<Control>();
			AddChild(SettingsInstance);

			// Let the settings menu decide its own ProcessMode (it sets Always in _Ready)
			// So DO NOT change SettingsInstance.ProcessMode here.
		}

		// Just ensure it's visible when button is pressed
		SettingsInstance.Visible = true;
	}
}
