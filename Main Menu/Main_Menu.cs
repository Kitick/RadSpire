//Purpose: Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

using System;
using Godot;
using SaveSystem;

public partial class Main_Menu : Control {
	//Main Buttons Panel References
	private Button StartButton = null!;
	private Button SettingsButton = null!;
	private Button QuitButton = null!;

	//Pop-up Panel References
	private Control StartButtonPanel = null!;
	private Button OnlineButton = null!;
	private Button LocalButton = null!;
	private Button PrivateMatchButton = null!;

	//Local Overlay Panel Button References
	private Button ContinueButton = null!;
	private Button LoadSavedButton = null!;
	private Button StartNewButton = null!;
	private Button BackToMainButton = null!;

	[Export] public PackedScene SettingsMenu = null!;
	private Control SettingsInstance = null!;

	public override void _Ready() {
		SettingsMenu = GD.Load<PackedScene>("res://Settings Menu/Settings_Menu.tscn");
		GetComponents();
		SetCallbacks();
	}

	private void GetComponents() {
		//Main Buttons
		StartButton = GetNode<Button>("Main_Button_Panel/Start_Button");
		SettingsButton = GetNode<Button>("Main_Button_Panel/Settings_Button");
		QuitButton = GetNode<Button>("Main_Button_Panel/Quit_Button");

		//Pop-up Panel Buttons
		StartButtonPanel = GetNode<Control>("Start_Button_Panel");
		OnlineButton = GetNode<Button>("Start_Button_Panel/Online_Button");
		LocalButton = GetNode<Button>("Start_Button_Panel/Local_Button");
		PrivateMatchButton = GetNode<Button>("Start_Button_Panel/Private_Match_Button");

		//Local Screen Overlay Buttons
		ContinueButton = GetNode<Button>("Local_Overlay_Panel/ColorRect/VBoxContainer/Continue_Button");
		LoadSavedButton = GetNode<Button>("Local_Overlay_Panel/ColorRect/VBoxContainer/Load_Saved_Button");
		StartNewButton = GetNode<Button>("Local_Overlay_Panel/ColorRect/VBoxContainer/Start_New_Button");
		BackToMainButton = GetNode<Button>("Local_Overlay_Panel/ColorRect/VBoxContainer/Back_To_Main_Button");
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
		OnlineButton.Pressed += StartOnlineGame;
		LocalButton.Pressed += StartLocalGame;
		PrivateMatchButton.Pressed += StartPrivateMatch;

		//Local Overlay Buttons
		ContinueButton.Pressed += OnContinueButtonPressed;
		LoadSavedButton.Pressed += OnLoadSavedButtonPressed;
		StartNewButton.Pressed += OnStartNewButtonPressed;
		BackToMainButton.Pressed += OnBackToMainButtonPressed;
	}

	//Main Button Handlers
	private void OnStartButtonPressed() {
		GD.Print("Start button was pressed!");
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

	// private void OnSettingsButtonPressed() {
	// 	GD.Print("Settings button was pressed!");
	// 	GetTree().ChangeSceneToFile("res://Settings Menu/Settings_Menu.tscn");
	// }

	private void OnQuitButtonPressed() {
		GD.Print("Quit button was pressed!");
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

			if(!insideStartButton && !insidePanel) { StartButtonPanel.Visible = false; }
		};
	}

	private void OnStartButtonUnhover() {
		HidePopup();
	}

	private void OnPanelMouseExited() {
		HidePopup();
	}

	private enum MenuMode { Online, Local, PrivateMatch }

	//Pop-up panel Buttons Handler
	private void StartOnlineGame() {
		GD.Print("Starting Online Game...");
	}

	private void StartLocalGame() {
		GD.Print("Starting Local Game...");
		GetNode<Control>("Local_Overlay_Panel").Visible = true;
		GetNode<Control>("Start_Button_Panel").Visible = false;
	}

	private void StartPrivateMatch() {
		GD.Print("Starting Private Match...");
	}

	//Local Overlay Buttons Handler
	private void OnContinueButtonPressed() {
		GD.Print("Continue Game Button was pressed!");
		GameManager.ShouldLoad = true;
		LoadGameScene();
	}

	private void OnLoadSavedButtonPressed() {
		var saves = SaveService.ListSaves();

		GD.Print("Available Saves:");
		foreach(var save in saves) { GD.Print(save); }
	}

	private void OnStartNewButtonPressed() {
		GameManager.ShouldLoad = false;
		LoadGameScene();
	}

	private void LoadGameScene() {
		GetTree().ChangeSceneToFile("res://Initial Scene/initial_player_scene.tscn");
	}

	private void OnBackToMainButtonPressed() {
		GD.Print("Back button was pressed!");
		GetNode<Control>("Local_Overlay_Panel").Visible = false;
		GetNode<Control>("Start_Button_Panel").Visible = true;
	}
}