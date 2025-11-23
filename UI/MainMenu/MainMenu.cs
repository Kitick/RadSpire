//Purpose: Main Menu Layout and Pop-up Panels

using Core;
using Godot;
using SaveSystem;

public partial class MainMenu : Control {
	//Paths for all Buttons and Pop-up Panels

	//Initial Main Menu buttons
	private const string SINGLEPLAYER_BUTTON = "Main_Button_Panel/Singleplayer_Button";
	private const string MULTIPLAYER_BUTTON = "Main_Button_Panel/Multiplayer_Button";
	private const string SETTINGS_BUTTON = "Main_Button_Panel/Settings_Button";
	private const string EXTRAS_BUTTON = "Main_Button_Panel/Extras_Button";
	private const string QUIT_BUTTON = "Main_Button_Panel/Quit_Button";

	// Pop-up panel for Singleplayer
	private const string SINGLEPLAYER_BUTTON_PANEL = "Singleplayer_Button_Panel";
	private const string CONTINUE_BUTTON = "Singleplayer_Button_Panel/Continue_Button";
	private const string LOAD_SAVED_BUTTON = "Singleplayer_Button_Panel/Load_Saved_Button";
	private const string START_NEW_BUTTON = "Singleplayer_Button_Panel/Start_New_Button";

	// Pop-up panel for Multiplayer
	private const string MULTIPLAYER_BUTTON_PANEL = "Multiplayer_Button_Panel";
	private const string HOST_NEW_BUTTON = "Multiplayer_Button_Panel/Host_New_Button";
	private const string HOST_SAVED_BUTTON = "Multiplayer_Button_Panel/Host_Saved_Button";
	private const string JOIN_GAME_BUTTON = "Multiplayer_Button_Panel/Join_Game_Button";

	// Component references
	private Button SingleplayerButton = null!;
	private Control SingleplayerButtonPanel = null!;
	private Button MultiplayerButton = null!;
	private Control MultiplayerButtonPanel = null!;
	private Control SettingsInstance = null!;

	// Main
	public override void _Ready() {
		GetComponents();
		InitSettings();
		SetCallbacks();
	}

	private void InitSettings() {
		PackedScene SettingsMenu = GD.Load<PackedScene>(Scenes.SettingsMenu);

		SettingsInstance = SettingsMenu.Instantiate<Control>();
		SettingsInstance.Visible = false;

		AddChild(SettingsInstance);
	}

	// Components
	private void GetComponents() {
		// Singleplayer Components
		SingleplayerButton = GetNode<Button>(SINGLEPLAYER_BUTTON);
		SingleplayerButtonPanel = GetNode<Control>(SINGLEPLAYER_BUTTON_PANEL);

		// Multiplayer Components
		MultiplayerButton = GetNode<Button>(MULTIPLAYER_BUTTON);
		MultiplayerButtonPanel = GetNode<Control>(MULTIPLAYER_BUTTON_PANEL);
	}

	// Callbacks
	private void SetCallbacks() {
		// Initial Main Menu buttons
		SingleplayerButton.Pressed += OnSingleplayerButtonPressed;
		MultiplayerButton.Pressed += OnMultiplayerButtonPressed;
		GetNode<Button>(SETTINGS_BUTTON).Pressed += OnSettingsButtonPressed;
		GetNode<Button>(EXTRAS_BUTTON).Pressed += OnExtrasButtonPressed;
		GetNode<Button>(QUIT_BUTTON).Pressed += OnQuitButtonPressed;

		// Hover behavior for Singleplayer
		SingleplayerButton.MouseEntered += OnSingleplayerButtonHover;
		SingleplayerButton.MouseExited += OnSingleplayerButtonUnhover;
		SingleplayerButtonPanel.MouseExited += OnSingleplayerPanelMouseExited;

		// Hover behavior for Multiplayer
		MultiplayerButton.MouseEntered += OnMultiplayerButtonHover;
		MultiplayerButton.MouseExited += OnMultiplayerButtonUnhover;
		MultiplayerButtonPanel.MouseExited += OnMultiplayerPanelMouseExited;

		// Popup buttons for Singleplayer
		GetNode<Button>(CONTINUE_BUTTON).Pressed += OnContinueButtonPressed;
		GetNode<Button>(LOAD_SAVED_BUTTON).Pressed += OnLoadSavedButtonPressed;
		GetNode<Button>(START_NEW_BUTTON).Pressed += OnStartNewButtonPressed;

		// Popup buttons for Multiplayer
		GetNode<Button>(HOST_NEW_BUTTON).Pressed += OnHostNewButtonPressed;
		GetNode<Button>(HOST_SAVED_BUTTON).Pressed += OnHostSavedButtonPressed;
		GetNode<Button>(JOIN_GAME_BUTTON).Pressed += OnJoinGameButtonPressed;
	}

	// Main Menu Button Handlers
	private void OnSingleplayerButtonPressed() {
		GD.Print("Singleplayer button was pressed!");
	}

	private void OnMultiplayerButtonPressed() {
		GD.Print("Multiplayer button was pressed!");
	}

	private void OnSettingsButtonPressed() {
		GD.Print("Settings button was pressed!");
		SettingsInstance.Visible = true;
	}

	private void OnExtrasButtonPressed() {
		GD.Print("Extras button was pressed!");
	}

	private void OnQuitButtonPressed() {
		GD.Print("Quit button was pressed!");
		GetTree().Quit();
	}

	// Hover Pop-Up Logic
	private void OnSingleplayerButtonHover() {
		SingleplayerButtonPanel.Visible = true;
	}

	private void OnMultiplayerButtonHover() {
		MultiplayerButtonPanel.Visible = true;
	}

	// Unhover Pop-Up Logic
	private void HidePopup(double delay = 0.05) {
		GetTree().CreateTimer(delay).Timeout += () => {
			Vector2 mousePos = GetViewport().GetMousePosition();

			// Singleplayer
			bool insideSingleplayerButton = SingleplayerButton.GetGlobalRect().HasPoint(mousePos);
			bool insideSingleplayerPanel = SingleplayerButtonPanel.GetGlobalRect().HasPoint(mousePos);

			if(!insideSingleplayerButton && !insideSingleplayerPanel) {
				SingleplayerButtonPanel.Visible = false;
			}

			// Multiplayer
			bool insideMultiplayerButton = MultiplayerButton.GetGlobalRect().HasPoint(mousePos);
			bool insideMultiplayerPanel = MultiplayerButtonPanel.GetGlobalRect().HasPoint(mousePos);

			if(!insideMultiplayerButton && !insideMultiplayerPanel) {
				MultiplayerButtonPanel.Visible = false;
			}
		};
	}

	// Singleplayer Hide Pop-ups
	private void OnSingleplayerButtonUnhover() => HidePopup();
	private void OnSingleplayerPanelMouseExited() => HidePopup();

	// Multiplayer Hide Pop-ups
	private void OnMultiplayerButtonUnhover() => HidePopup();
	private void OnMultiplayerPanelMouseExited() => HidePopup();

	// Pop-up panel buttons handler for Singleplayer
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

	// Pop-up panel buttons handler for Multiplayer
	private void OnHostNewButtonPressed() {
		GD.Print("Host New Game Button Pressed!");
	}

	private void OnHostSavedButtonPressed() {
		GD.Print("Host Saved Button Pressed!");
	}

	private void OnJoinGameButtonPressed() {
		GD.Print("Join Game Button Pressed!");
	}

	// Load a new game scene
	private void LoadGameScene() {
		GetTree().ChangeSceneToFile(Scenes.GameScene);
	}

}