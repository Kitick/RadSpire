//Purpose: Main Menu Layout and Pop-up Panels

using Core;
using Godot;
using Network;
using SaveSystem;
using Settings;
using MultiplayerPanels;
using LoadMenuScene;

public partial class MainMenu : Control {
	public static readonly bool Debug = false;

	enum MenuState { Normal, SinglePopup, MultiPopup }

	private const float HideDelay = 0.25f;

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
	//Load Scene Reference
	private HostPanel HostPanel = null!;

	// Main
	public override void _Ready() {
		LoadScenes();
		GetComponents();
		SetCallbacks();
		SubscribeToNetworkEvents();
	}

	public override void _ExitTree() {
		UnsubscribeFromNetworkEvents();
	}

	private void SubscribeToNetworkEvents() {
		Server.Instance.OnJoinedServer += OnJoinedServer;
		Server.Instance.OnHostStarted += OnHostStarted;
	}

	private void UnsubscribeFromNetworkEvents() {
		Server.Instance.OnJoinedServer -= OnJoinedServer;
		Server.Instance.OnHostStarted -= OnHostStarted;
	}

	private void OnJoinedServer() {
		GD.Print("[MainMenu] Successfully joined server, starting game...");
		GameManager.Instance.StartGame();
	}

	private void OnHostStarted() {
		GD.Print("[MainMenu] Host started successfully");
	}

	//Load Scenes
	public void LoadScenes() {
		var packed1 = GD.Load<PackedScene>("res://UI/MultiplayerPanels/HostPanel/HostPanel.tscn");
        HostPanel = packed1.Instantiate<HostPanel>();
		HostPanel.Visible = false;
        AddChild(HostPanel);
	}

	// Components
	private void GetComponents() {
		// Singleplayer Components
		SingleplayerButton = GetNode<Button>(SINGLEPLAYER_BUTTON);
		SingleplayerButtonPanel = GetNode<Control>(SINGLEPLAYER_POPUP);

		// Multiplayer Components
		MultiplayerButton = GetNode<Button>(MULTIPLAYER_BUTTON);
		MultiplayerButtonPanel = GetNode<Control>(MULTIPLAYER_POPUP);
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
		SingleplayerButton.MouseEntered += () => SetState(MenuState.SinglePopup);
		SingleplayerButton.MouseExited += HidePopup;
		SingleplayerButtonPanel.MouseExited += HidePopup;

		// Hover behavior for Multiplayer
		MultiplayerButton.MouseEntered += () => SetState(MenuState.MultiPopup);
		MultiplayerButton.MouseExited += HidePopup;
		MultiplayerButtonPanel.MouseExited += HidePopup;

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
		if(Debug) { GD.Print($"MainMenu: Setting Menu State to {state}"); }

		SingleplayerButtonPanel.Visible = state == MenuState.SinglePopup;
		MultiplayerButtonPanel.Visible = state == MenuState.MultiPopup;
	}

	// Main Menu Button Handlers
	private void OnSingleplayerButtonPressed() {

	}

	private void OnMultiplayerButtonPressed() {

	}

	private void OnSettingsButtonPressed() {
		var settings = this.AddScene<SettingsMenu>(Scenes.SettingsMenu);
		settings.OpenMenu();
	}

	private void OnExtrasButtonPressed() {

	}

	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}

	// Unhover Pop-Up Logic
	private bool IsMouseInside(params Control[] nodes) {
		Vector2 mousePos = GetViewport().GetMousePosition();

		bool IsInside = false;
		foreach(var node in nodes) {
			if(node.GetGlobalRect().HasPoint(mousePos)) {
				IsInside = true;
				break;
			}
		}

		return IsInside;
	}

	private void HidePopup() {
		GetTree().CreateTimer(HideDelay).Timeout += () => {
			if(!IsMouseInside(SingleplayerButton, SingleplayerButtonPanel, MultiplayerButton, MultiplayerButtonPanel)) {
				SetState(MenuState.Normal);
			}
		};
	}

	// Pop-up panel buttons handler for Singleplayer
	private void OnContinueButtonPressed() {
		GameManager.Instance.Load("autosave");
		GameManager.Instance.StartGame();
	}

	private void OnLoadSavedButtonPressed() {
		var saves = SaveService.ListSaves();

		GD.Print("Available Saves:");
		foreach(var save in saves) { GD.Print(save); }
	}

	private void OnStartNewButtonPressed() {
		GameManager.Instance.StartGame();
	}

	// Pop-up panel buttons handler for Multiplayer
	private void OnHostNewButtonPressed() {
		var host = this.AddScene<HostPanel>(Scenes.HostPanel);
		host.OpenMenu();

		HostPanel.UpdateHostText("Host New Game");
	}

	private void OnHostSavedButtonPressed() {

	}

	private void OnJoinGameButtonPressed() {
		var join = this.AddScene<JoinPanel>(Scenes.JoinPanel);
		join.OpenMenu();
	}

	// Load a new game scene
	private void LoadGameScene() {
		GetTree().ChangeSceneToFile(Scenes.GameScene);
	}
}