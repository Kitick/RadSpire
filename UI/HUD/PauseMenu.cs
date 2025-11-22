using SettingsPanels;
using Core;
using Godot;

public sealed partial class PauseMenu : Control {
	public Button ResumeButton = null!;
	public Button SettingsButton = null!;
	public Button SaveButton = null!;
	public Button MainMenuButton = null!;

	private const string RESUME_BUTTON = "Background/Buttons/Resume";
	private const string SETTINGS_BUTTON = "Background/Buttons/Settings";
	private const string SAVE_BUTTON = "Background/Buttons/Save";
	private const string MAIN_MENU_BUTTON = "Background/Buttons/Main_Menu";

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.WhenPaused;

		GetComponenets();
	}

	private void GetComponenets() {
		ResumeButton = GetNode<Button>(RESUME_BUTTON);
		SettingsButton = GetNode<Button>(SETTINGS_BUTTON);
		SaveButton = GetNode<Button>(SAVE_BUTTON);
		MainMenuButton = GetNode<Button>(MAIN_MENU_BUTTON);
	}
}
