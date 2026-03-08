using Core;
using Godot;
using Services.Network;

namespace UI {
	public sealed partial class PauseMenu : Control {
		[ExportCategory("Buttons")]
		[Export] public Button ResumeButton = null!;
		[Export] public Button SaveButton = null!;
		[Export] public Button HostButton = null!;
		[Export] public Button SettingsButton = null!;
		[Export] public Button MainMenuButton = null!;

		public override void _Ready() {
			this.ValidateExports();
			ProcessMode = ProcessModeEnum.WhenPaused;
		}

		public void OpenMenu() {
			UpdateHostButtonText();
			Visible = true;
			ResumeButton.GrabFocus();
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
}