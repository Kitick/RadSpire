namespace UI.RespawnMenu;

using Godot;
using UI;

public partial class RespawnMenu : BaseUIControl {
	public Button RespawnButton = null!;
	public Button MainMenuButton = null!;

	private const string BUTTONS = "DeadBG/Buttons";

	private const string RESPAWN_BUTTON = $"{BUTTONS}/Respawn";
	private const string MAIN_MENU = $"{BUTTONS}/Main_Menu";

	protected override Control? DefaultFocus => RespawnButton;

	public override void _Ready() {
		base._Ready();
		GetComponents();
	}

	private void GetComponents() {
		RespawnButton = GetNode<Button>(RESPAWN_BUTTON);
		MainMenuButton = GetNode<Button>(MAIN_MENU);
	}

	public void OpenMenu() {
		Visible = true;
		OnOpen();
	}

	public void CloseMenu() {
		Visible = false;
	}
}
