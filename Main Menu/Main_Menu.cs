//Purpose: Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

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
		StartButton = GetNode<Button>("Main_Button_Panel/Start_Button");
		SettingsButton = GetNode<Button>("Main_Button_Panel/Settings_Button");
		QuitButton = GetNode<Button>("Main_Button_Panel/Quit_Button");

		// Popup Panel and its Buttons
		StartButtonPanel = GetNode<Control>("Start_Button_Panel");
		OnlineButton = GetNode<Button>("Start_Button_Panel/Online_Button");
		LocalButton = GetNode<Button>("Start_Button_Panel/Local_Button");
		PrivateMatchButton = GetNode<Button>("Start_Button_Panel/Private_Match_Button");
	}

	private void SetCallbacks() {
		//Main Buttons
		StartButton.Pressed += OnStartButtonPressed;
		SettingsButton.Pressed += OnSettingsButtonPressed;
		QuitButton.Pressed += OnQuitButtonPressed;

		//Hover Behavior
		StartButton.MouseEntered += OnStartButtonHover;
		StartButton.MouseExited += OnStartButtonUnhover;
		StartButtonPanel.MouseExited += OnPanelMouseExited;

		//Click Handlers for Pop-up
		OnlineButton.Pressed += () => OnModeSelected(MenuMode.Online);
		LocalButton.Pressed += () => OnModeSelected(MenuMode.Local);
		PrivateMatchButton.Pressed += () => OnModeSelected(MenuMode.PrivateMatch);
	}

	//Main Button Handlers
	private void OnStartButtonPressed() {
		GD.Print("Start button was pressed!");
	}

	private void OnSettingsButtonPressed() {
		GD.Print("Settings button was pressed");
	}

	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}

	//Hover Pop-Up Logic
	private void OnStartButtonHover() {
		StartButtonPanel.Visible = true;
	}

	//Unhover Pop-Up Logic
	private void HidePopup(double delay = 0.2) {
		GetTree().CreateTimer(delay).Timeout += () => {
			Vector2 mousePos = GetViewport().GetMousePosition();
			bool insideStartButton = StartButton.GetGlobalRect().HasPoint(mousePos);
			bool insidePanel = StartButtonPanel.GetGlobalRect().HasPoint(mousePos);

			if(!insideStartButton && !insidePanel){ StartButtonPanel.Visible = false; }
		};
	}

	private void OnStartButtonUnhover() {
		HidePopup();
	}

	private void OnPanelMouseExited() {
		HidePopup();
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