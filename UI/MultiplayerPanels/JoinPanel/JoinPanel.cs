using System;
using Godot;
using InputSystem;
using Network;

namespace MultiplayerPanels {
	public partial class JoinPanel : Control {
		private static readonly Logger Log = new(nameof(JoinPanel), enabled: true);

		// Paths for Panel Attributes
		private const string PANEL_AREA = "PanelArea";
		private const string NO_PASSWORD_CHECKBOX = PANEL_AREA + "/NoPassword/NoPasswordCheckBox";
		private const string NOT_FULL_CHECKBOX = PANEL_AREA + "/NotFull/NotFullCheckBox";
		private const string CANCEL_BUTTON = PANEL_AREA + "/CancelButton";
		private const string JOIN_BUTTON = PANEL_AREA + "/JoinButton";
		private const string IP_ADDRESS_INPUT = PANEL_AREA + "/VLineEditContainer/IPAddressDirect";
		private const string STATUS_LABEL = PANEL_AREA + "/JoinGame";

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
			Log.Info($"No Password Toggled: {check}");
		}

		private void OnNotFullCheckboxToggled(bool check) {
			//Implementation Here
			Log.Info($"Not Full Toggled: {check}");
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

			if(string.IsNullOrWhiteSpace(address)) {
				UpdateStatus("Please enter an IP address");
				return;
			}

			UpdateStatus("Connecting...");
			Error result = Server.Instance.Join(address);

			if(result != Error.Ok) {
				Log.Error($"Failed to join: {result}");
				UpdateStatus($"Failed to connect: {result}");
			}
			else {
				Log.Info($"Attempting to join {address}...");
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