//Purpose: Simple Main Menu Screen Layout and Buttons with Hover Popup Panel

using Constants;
using Godot;
using SaveSystem;

public partial class Main_Menu : Control {
	// Main button paths
	private const string START_BUTTON = "Main_Button_Panel/Start_Button";
	private const string SETTINGS_BUTTON = "Main_Button_Panel/Settings_Button";
	private const string QUIT_BUTTON = "Main_Button_Panel/Quit_Button";

	// Popup panel paths
	private const string START_PANEL = "Start_Button_Panel";
	private const string ONLINE_BUTTON = "Start_Button_Panel/Online_Button";
	private const string LOCAL_BUTTON = "Start_Button_Panel/Local_Button";
	private const string PRIVATE_BUTTON = "Start_Button_Panel/Private_Match_Button";

	// Local overlay paths
	private const string LOCAL_OVERLAY = "Local_Overlay_Panel";
	private const string CONTINUE_BUTTON = "Local_Overlay_Panel/ColorRect/VBoxContainer/Continue_Button";
	private const string LOAD_SAVED_BUTTON = "Local_Overlay_Panel/ColorRect/VBoxContainer/Load_Saved_Button";
	private const string START_NEW_BUTTON = "Local_Overlay_Panel/ColorRect/VBoxContainer/Start_New_Button";
	private const string BACK_TO_MAIN_BUTTON = "Local_Overlay_Panel/ColorRect/VBoxContainer/Back_To_Main_Button";

	// Component references
	private Button StartButton = null!;
	private Control StartButtonPanel = null!;
	private Control LocalOverlayPanel = null!;

	private Control SettingsInstance = null!;

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
		StartButton = GetNode<Button>(START_BUTTON);

		StartButtonPanel = GetNode<Control>(START_PANEL);
		LocalOverlayPanel = GetNode<Control>(LOCAL_OVERLAY);
	}

	private void SetCallbacks() {
		// Main buttons
		StartButton.Pressed += OnStartButtonPressed;
		GetNode<Button>(SETTINGS_BUTTON).Pressed += OnSettingsButtonPressed;
		GetNode<Button>(QUIT_BUTTON).Pressed += OnQuitButtonPressed;

		// Hover behavior
		StartButton.MouseEntered += OnStartButtonHover;
		StartButton.MouseExited += OnStartButtonUnhover;
		StartButtonPanel.MouseExited += OnPanelMouseExited;

		// Popup buttons
		GetNode<Button>(ONLINE_BUTTON).Pressed += StartOnlineGame;
		GetNode<Button>(LOCAL_BUTTON).Pressed += StartLocalGame;
		GetNode<Button>(PRIVATE_BUTTON).Pressed += StartPrivateMatch;

		// Local overlay buttons
		GetNode<Button>(CONTINUE_BUTTON).Pressed += OnContinueButtonPressed;
		GetNode<Button>(LOAD_SAVED_BUTTON).Pressed += OnLoadSavedButtonPressed;
		GetNode<Button>(START_NEW_BUTTON).Pressed += OnStartNewButtonPressed;
		GetNode<Button>(BACK_TO_MAIN_BUTTON).Pressed += OnBackToMainButtonPressed;
	}

	//Main Button Handlers
	private void OnStartButtonPressed() {
		GD.Print("Start button was pressed!");
	}

	private void OnSettingsButtonPressed() {
		SettingsInstance.Visible = true;
	}

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

			if(!insideStartButton && !insidePanel) {
				StartButtonPanel.Visible = false;
			}
		};
	}

	private void OnStartButtonUnhover() => HidePopup();
	private void OnPanelMouseExited() => HidePopup();

	//Pop-up panel Buttons Handler
	private void StartOnlineGame() {
		GD.Print("Starting Online Game...");
	}

	private void StartLocalGame() {
		GD.Print("Starting Local Game...");
		LocalOverlayPanel.Visible = true;
		StartButtonPanel.Visible = false;
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
		GetTree().ChangeSceneToFile(Scenes.GameScene);
	}

	private void OnBackToMainButtonPressed() {
		GD.Print("Back button was pressed!");
		LocalOverlayPanel.Visible = false;
		StartButtonPanel.Visible = true;
	}
}