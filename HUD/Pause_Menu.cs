using Godot;
using System;

public partial class Pause_Menu : Control {
	
	private Button ResumeButton = null!;
	private Button SettingsButton = null!;
	private Button QuitButton = null!;

	public override void _Ready() {
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		GetComponents();
		SetCallBacks();
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		// Only close on Esc if the menu is currently shown
		if (Visible && @event.IsActionPressed("ui_cancel"))
		{
			OnResumeButtonPressed();
			GetViewport().SetInputAsHandled(); // stop further Esc handling this frame
		}
	}

	private void GetComponents() {
		
		ResumeButton = GetNode<Button>("Pause_Buttons/Resume");
		SettingsButton = GetNode<Button>("Pause_Buttons/Settings");
		QuitButton = GetNode<Button>("Pause_Buttons/Quit");
	}

	private void SetCallBacks() {
		ResumeButton.Pressed += OnResumeButtonPressed;
		SettingsButton.Pressed += OnSettingsButtonPressed;
		QuitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnResumeButtonPressed() {
		GetTree().Paused = false;
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnSettingsButtonPressed() {
		
	}

	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}
