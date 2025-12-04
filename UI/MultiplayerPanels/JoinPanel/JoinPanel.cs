using System;
using Godot;
using InputSystem;
using Network;

namespace MultiplayerPanels {
	public partial class JoinPanel : Control {

		// Paths for Panel Attributes
		private const string NO_PASSWORD_CHECKBOX = "PanelArea/NoPassword/NoPasswordCheckBox";
		private const string NOT_FULL_CHECKBOX = "PanelArea/NotFull/NotFullCheckBox";
		private const string CANCEL_BUTTON = "PanelArea/CancelButton";
		private const string JOIN_BUTTON = "PanelArea/JoinButton";
		private const string IP_ADDRESS_INPUT = "PanelArea/VLineEditContainer/IPAddressDirect";
		private const string STATUS_LABEL = "PanelArea/JoinGame";

		// Component References
		private LineEdit inputIPAddress = null!;
		private Label statusLabel = null!;

		public event Action? OnMenuClosed;
		private event Action? OnExit;

		// Main
		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			GetComponents();
			SetCallbacks();
			SetInputCallbacks();
		}

		private void GetComponents() {
			inputIPAddress = GetNode<LineEdit>(IP_ADDRESS_INPUT);
			statusLabel = GetNode<Label>(STATUS_LABEL);
		}

		private void SetCallbacks() {
			GetNode<CheckBox>(NO_PASSWORD_CHECKBOX).Toggled += OnNoPasswordCheckboxToggled;
			GetNode<CheckBox>(NOT_FULL_CHECKBOX).Toggled += OnNotFullCheckboxToggled;
			GetNode<Button>(CANCEL_BUTTON).Pressed += OnCancelButtonPressed;
			GetNode<Button>(JOIN_BUTTON).Pressed += OnJoinButtonPressed;
			inputIPAddress.TextSubmitted += OnIPAddressSubmitted;
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		// CallBacks
		private void OnNoPasswordCheckboxToggled(bool check) {
			//Implementation Here
			GD.Print($"No Password Toggled: {check}");
		}

		private void OnNotFullCheckboxToggled(bool check) {
			//Implementation Here
			GD.Print($"No Password Toggled: {check}");
		}
		private void OnCancelButtonPressed() {
			CloseMenu();
		}

		private void OnJoinButtonPressed() {
			AttemptJoin();
		}

		private void OnIPAddressSubmitted(string text) {
			AttemptJoin();
		}

		private void AttemptJoin() {
			string address = inputIPAddress.Text.Trim();

			if (string.IsNullOrWhiteSpace(address)) {
				UpdateStatus("Please enter an IP address");
				return;
			}

			UpdateStatus("Connecting...");
			Error result = Server.Instance.Join(address);

			if (result != Error.Ok) {
				GD.PrintErr($"[JoinPanel] Failed to join: {result}");
				UpdateStatus($"Failed to connect: {result}");
			} else {
				GD.Print($"[JoinPanel] Attempting to join {address}...");
				// Success will be handled by Server.OnJoinedServer event in MainMenu/HUD
			}
		}

		private void UpdateStatus(string message) {
			statusLabel.Text = message;
		}

		public void OpenMenu() {
			Visible = true;
		}

		public void CloseMenu() {
			QueueFree();
		}


	}
}