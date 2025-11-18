//Purpose: Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

using Core;
using Godot;
using SaveSystem;

public partial class Main_Menu : Control {
	// Main button paths

	private const string SINGLEPLAYER_BUTTON = "Main_Button_Panel/Singleplayer_Button";
	private const string MULTIPLAYER_BUTTON = "Main_Button_Panel/Multiplayer_Button";
	private const string SETTINGS_BUTTON = "Main_Button_Panel/Settings_Button";
	private const string QUIT_BUTTON = "Main_Button_Panel/Quit_Button";

	// Popup panel paths
	private const string SINGLEPLAYER_BUTTON_PANEL = "Singleplayer_Button_Panel";
	private const string CONTINUE_BUTTON = "Singleplayer_Button_Panel/Continue_Button";
	private const string LOAD_SAVED_BUTTON = "Singleplayer_Button_Panel/Load_Saved_Button";
	private const string START_NEW_BUTTON = "Singleplayer_Button_Panel/Start_New_Button";


	// Component references
	private Button SingleplayerButton = null!;
	private Control SingleplayerButtonPanel = null!;
	private Button MultiplayerButton = null!;

	private Control SettingsInstance = null!;

	//Main
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

	private void GetComponents() {
		// Components
		SingleplayerButton = GetNode<Button>(SINGLEPLAYER_BUTTON);
		SingleplayerButtonPanel = GetNode<Control>(SINGLEPLAYER_BUTTON_PANEL);
		MultiplayerButton = GetNode<Button>(MULTIPLAYER_BUTTON);
	}

	private void SetCallbacks() {
		// Main buttons
		SingleplayerButton.Pressed += OnSingleplayerButtonPressed;
		MultiplayerButton.Pressed += OnMultiplayerButtonPressed;
		GetNode<Button>(SETTINGS_BUTTON).Pressed += OnSettingsButtonPressed;
		GetNode<Button>(QUIT_BUTTON).Pressed += OnQuitButtonPressed;

		// Hover behavior
		SingleplayerButton.MouseEntered += OnSingleplayerButtonHover;
		SingleplayerButton.MouseExited += OnSingleplayerButtonUnhover;
		SingleplayerButtonPanel.MouseExited += OnPanelMouseExited;

		// Popup buttons
		GetNode<Button>(CONTINUE_BUTTON).Pressed += OnContinueButtonPressed;
		GetNode<Button>(LOAD_SAVED_BUTTON).Pressed += OnLoadSavedButtonPressed;
		GetNode<Button>(START_NEW_BUTTON).Pressed += OnStartNewButtonPressed;
	}

	//Main Button Handlers
	private void OnSingleplayerButtonPressed() {
		GD.Print("Singleplayer button was pressed!");
	}

	private void OnMultiplayerButtonPressed() {
		GD.Print("Multiplayer button was pressed!");
	}

	private void OnSettingsButtonPressed() {
		SettingsInstance.Visible = true;
	}

	private void OnQuitButtonPressed() {
		GD.Print("Quit button was pressed!");
		GetTree().Quit();
	}

	//Hover Pop-Up Logic
	private void OnSingleplayerButtonHover() {
		SingleplayerButtonPanel.Visible = true;
	}

	//Unhover Pop-Up Logic
	private void HidePopup(double delay = 0.2) {
		GetTree().CreateTimer(delay).Timeout += () => {
			Vector2 mousePos = GetViewport().GetMousePosition();

			bool insideSingleplayerButton = SingleplayerButton.GetGlobalRect().HasPoint(mousePos);
			bool insidePanel = SingleplayerButtonPanel.GetGlobalRect().HasPoint(mousePos);

			if(!insideSingleplayerButton && !insidePanel) {
				SingleplayerButtonPanel.Visible = false;
			}
		};
	}

	private void OnSingleplayerButtonUnhover() => HidePopup();
	private void OnPanelMouseExited() => HidePopup();

	//Pop-up panel Buttons Handler
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

	//Load a New game scene
	private void LoadGameScene() {
		GetTree().ChangeSceneToFile(Scenes.GameScene);
	}

}