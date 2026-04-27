namespace UI.PauseMenu;

using Godot;
using Network;
using Root;
using UI;

public sealed partial class PauseMenu : BaseUIControl {
	[ExportCategory("Buttons")]
	[Export] public Button ResumeButton = null!;
	[Export] public Button SaveButton = null!;
	[Export] public Button HostButton = null!;
	[Export] public Button SettingsButton = null!;
	[Export] public Button MainMenuButton = null!;

	protected override Button? DefaultFocus => ResumeButton;

	public override void _Ready() {
		base._Ready();
		this.ValidateExports();
		ProcessMode = ProcessModeEnum.WhenPaused;
	}

	public void OpenMenu() {
		UpdateHostButtonText();
		Visible = true;
		OnOpen();
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
