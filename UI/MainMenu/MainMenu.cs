//Purpose: Main Menu Layout and Pop-up Panels

using Core;
using Godot;
using SaveSystem;
using Settings;

public partial class MainMenu : Control {
	public static readonly bool Debug = false;

	enum MenuState { Normal, SinglePopup, MultiPopup }

	//Paths for all Buttons and Pop-up Panels

	//Initial Main Menu buttons
	private const string MAIN_BUTTON_PANEL = "Main_Button_Panel";
	private const string SINGLEPLAYER_BUTTON = MAIN_BUTTON_PANEL + "/Singleplayer_Button";
	private const string MULTIPLAYER_BUTTON = MAIN_BUTTON_PANEL + "/Multiplayer_Button";
	private const string SETTINGS_BUTTON = MAIN_BUTTON_PANEL + "/Settings_Button";
	private const string EXTRAS_BUTTON = MAIN_BUTTON_PANEL + "/Extras_Button";
	private const string QUIT_BUTTON = MAIN_BUTTON_PANEL + "/Quit_Button";

	// Pop-up panel for Singleplayer
	private const string SINGLEPLAYER_POPUP = SINGLEPLAYER_BUTTON + "/Singleplayer_Popup";
	private const string CONTINUE_BUTTON = SINGLEPLAYER_POPUP + "/Continue_Button";
	private const string LOAD_SAVED_BUTTON = SINGLEPLAYER_POPUP + "/Load_Saved_Button";
	private const string START_NEW_BUTTON = SINGLEPLAYER_POPUP + "/Start_New_Button";

	// Pop-up panel for Multiplayer
	private const string MULTIPLAYER_POPUP = MULTIPLAYER_BUTTON + "/Multiplayer_Popup";
	private const string HOST_NEW_BUTTON = MULTIPLAYER_POPUP + "/Host_New_Button";
	private const string HOST_SAVED_BUTTON = MULTIPLAYER_POPUP + "/Host_Saved_Button";
	private const string JOIN_GAME_BUTTON = MULTIPLAYER_POPUP + "/Join_Game_Button";

	// Component references
	private Button SingleplayerButton = null!;
	private Button MultiplayerButton = null!;
	private Control SingleplayerButtonPanel = null!;
	private Control MultiplayerButtonPanel = null!;

	private SettingsMenu Settings = null!;

	// Main
	public override void _Ready() {
		GetComponents();
		SetCallbacks();
	}

	// Components
	private void GetComponents() {
		// Singleplayer Components
		SingleplayerButton = GetNode<Button>(SINGLEPLAYER_BUTTON);
		SingleplayerButtonPanel = GetNode<Control>(SINGLEPLAYER_POPUP);

		// Multiplayer Components
		MultiplayerButton = GetNode<Button>(MULTIPLAYER_BUTTON);
		MultiplayerButtonPanel = GetNode<Control>(MULTIPLAYER_POPUP);

		// Settings Instance
		Settings = this.AddScene<SettingsMenu>(Scenes.SettingsMenu);
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

	private void SetState(MenuState state) {
		if(Debug){ GD.Print($"MainMenu: Setting Menu State to {state}"); }

		SingleplayerButtonPanel.Visible = state == MenuState.SinglePopup;
		MultiplayerButtonPanel.Visible = state == MenuState.MultiPopup;
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
		Settings.OpenMenu();
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
		SetState(MenuState.SinglePopup);
	}

	private void OnMultiplayerButtonHover() {
		SetState(MenuState.MultiPopup);
	}

	// Unhover Pop-Up Logic
	private bool IsMouseInside(Control button, Control panel) {
		Vector2 mousePos = GetViewport().GetMousePosition();

		bool isInsideButton = button.GetGlobalRect().HasPoint(mousePos);
		bool isInsidePanel = panel.GetGlobalRect().HasPoint(mousePos);

		return isInsideButton || isInsidePanel;
	}

	private void HidePopup(double delay = 0.5) {
		GetTree().CreateTimer(delay).Timeout += () => {
			if(!IsMouseInside(SingleplayerButton, SingleplayerButtonPanel)) {
				SetState(MenuState.Normal);
			}

			if(!IsMouseInside(MultiplayerButton, MultiplayerButtonPanel)) {
				SetState(MenuState.Normal);
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