// Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

using Godot;
using System;

public partial class Main_Menu : Control {
	//Button and Panel References

	private Button StartButton = null!;
	private Button SettingsButton = null!;
	private Button QuitButton = null!;

	private Control StartButtonPanel = null!;
	private Button OnlineButton = null!;
	private Button LocalButton = null!;
	private Button PrivateMatchButton = null!;

	public override void _Ready() {
		GetComponents();
		SetCallbacks();
	}

	private void GetComponents() {
		// Main Buttons
		StartButton = GetNode<Button>("VBoxContainer/Start_Button");
		SettingsButton = GetNode<Button>("VBoxContainer/Settings_Button");
		QuitButton = GetNode<Button>("VBoxContainer/Quit_Button");

		// Popup Panel and its Buttons
		StartButtonPanel = GetNode<Control>("Start_Button_Panel");
		OnlineButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Online_Button");
		LocalButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Local_Button");
		PrivateMatchButton = GetNode<Button>("Start_Button_Panel/VBoxContainer/Private_Match_Button");
	}

	private void SetCallbacks() {
		// Main Buttons
		StartButton.Pressed += OnStartButtonPressed;
		SettingsButton.Pressed += OnSettingsButtonPressed;
		QuitButton.Pressed += OnQuitButtonPressed;

		// Hover Behavior
		StartButton.MouseEntered += OnStartButtonHover;
		StartButton.MouseExited += OnStartButtonUnhover;
		StartButtonPanel.MouseExited += OnPanelMouseExited;

		// Click Handlers for Pop-up
		OnlineButton.Pressed += () => OnModeSelected(MenuMode.Online);
		LocalButton.Pressed += () => OnModeSelected(MenuMode.Local);
		PrivateMatchButton.Pressed += () => OnModeSelected(MenuMode.PrivateMatch);
	}

	// Main Button Handlers

	private void OnStartButtonPressed() {
		GD.Print("Start button was pressed!");
	}

	private void OnSettingsButtonPressed() {
		GD.Print("Settings button was pressed");
	}

	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}

	// Hover Pop-Up Logic

	private void OnStartButtonHover() {
		// Position the Pop-up next to the Start button

		Vector2 popupPosition = StartButton.GlobalPosition + new Vector2(StartButton.Size.X + -100, 150);
		StartButtonPanel.GlobalPosition = popupPosition;
		StartButtonPanel.Visible = true;
	}

	// Unhover Pop-Up Logic

	private void OnStartButtonUnhover() {
		//Wait a tiny bit before hiding, so the user can move the mouse into the panel

		GetTree().CreateTimer(0.1).Timeout += () => {
			Vector2 mousePos = GetViewport().GetMousePosition();

			//Only hide if the mouse is NOT inside the Start_Button_Panel area
			if(!StartButtonPanel.GetGlobalRect().HasPoint(mousePos))
				StartButtonPanel.Visible = false;
		};
	}

	private void OnPanelMouseExited() {
		//Wait a bit, then check if the mouse is back on the Start_Button

		GetTree().CreateTimer(0.1).Timeout += () => {
			Vector2 mousePos = GetViewport().GetMousePosition();

			//Only hide if mouse is outside BOTH Start_Button and Start_Button_Panel
			if(!StartButton.GetGlobalRect().HasPoint(mousePos) && !StartButtonPanel.GetGlobalRect().HasPoint(mousePos)) {
				StartButtonPanel.Visible = false;
			}
		};
	}

	private enum MenuMode { Online, Local, PrivateMatch }

	private void OnModeSelected(MenuMode mode) {
		GD.Print($"Selected mode: {mode}");
		StartButtonPanel.Visible = false;

		switch (mode) {
			case MenuMode.Online: StartOnlineGame(); break;
			case MenuMode.Local: StartLocalGame(); break;
			case MenuMode.PrivateMatch: StartPrivateMatch(); break;
		}
	}

	private void StartOnlineGame() {
		GD.Print("Starting Online Game...");
	}

	private void StartLocalGame() {
		GD.Print("Starting Local Game...");
	}

	private void StartPrivateMatch() {
		GD.Print("Starting Private Match...");
	}
}
