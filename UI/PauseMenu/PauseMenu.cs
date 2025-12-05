using Godot;
using Network;

public sealed partial class PauseMenu : Control {
	public Button ResumeButton = null!;
	public Button SettingsButton = null!;
	public Button SaveButton = null!;
	public Button HostButton = null!;
	public Button MainMenuButton = null!;

	private const string BUTTONS = "PanelArea/Buttons";

	private const string RESUME_BUTTON = $"{BUTTONS}/Resume";
	private const string SETTINGS_BUTTON = $"{BUTTONS}/Settings";
	private const string SAVE_BUTTON = $"{BUTTONS}/Save";
	private const string HOST_BUTTON = $"{BUTTONS}/Host";
	private const string MAIN_MENU_BUTTON = $"{BUTTONS}/Main_Menu";

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.WhenPaused;

		GetComponenets();
	}

	private void GetComponenets() {
		ResumeButton = GetNode<Button>(RESUME_BUTTON);
		SettingsButton = GetNode<Button>(SETTINGS_BUTTON);
		SaveButton = GetNode<Button>(SAVE_BUTTON);
		HostButton = GetNode<Button>(HOST_BUTTON);
		MainMenuButton = GetNode<Button>(MAIN_MENU_BUTTON);
	}

	public void OpenMenu() {
		UpdateHostButtonText();
		Visible = true;
	}

	public void CloseMenu() {
		Visible = false;
	}

	public void UpdateHostButtonText() {
		if(Server.Instance.IsNetworkConnected) {
			HostButton.Text = "DISCONNECT";
		}
		else {
			HostButton.Text = "HOST";
		}
	}
}
