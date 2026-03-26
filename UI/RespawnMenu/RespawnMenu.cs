namespace UI;

using Godot;

public partial class RespawnMenu : Control {
	public Button RespawnButton = null!;
	public Button MainMenuButton = null!;

	private const string BUTTONS = "DeadBG/Buttons";

	private const string RESPAWN_BUTTON = $"{BUTTONS}/Respawn";
	private const string MAIN_MENU = $"{BUTTONS}/Main_Menu";

	public override void _Ready() {
		GetComponents();
	}

	private void GetComponents() {
		RespawnButton = GetNode<Button>(RESPAWN_BUTTON);
		MainMenuButton = GetNode<Button>(MAIN_MENU);
	}

	public void OpenMenu() {
		Visible = true;
	}

	public void CloseMenu() {
		Visible = false;
	}
}
