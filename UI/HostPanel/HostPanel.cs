using System;
using Godot;
using InputSystem;

namespace MultiplayerPanels {
	public sealed partial class HostPanel : Control {

		//Paths For Panel Attributes
		private const string PANEL_AREA = "PanelArea";
		private const string LABEL_HOST_TEXT = PANEL_AREA + "/lblHostText";
		private const string PASSWORD_CHECKBOX = PANEL_AREA + "/PasswordContainer/lblPassword/PasswordCheckbox";
		private const string INPUT_GAME_NAME_TEXT = PANEL_AREA + "/GameNameContainer/InputGameName";
		private const string INPUT_PASSWORD_TEXT = PANEL_AREA + "/PasswordContainer/InputPassword";
		private const string CANCEL_BUTTON = PANEL_AREA + "/OptionContainer/CancelButton";
		private const string HOST_BUTTON = PANEL_AREA + "/OptionContainer/HostButton";

		//Component Reference
		Label hostText = null!;

		// Events
		public event Action? OnMenuClosed;
		private event Action? OnExit;

		// Override Functions
		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			GetComponents();

			SetCallbacks();
			SetInputCallbacks();

			UpdateHostText();
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		//Get Components
		private void GetComponents() {
			hostText = GetNode<Label>(LABEL_HOST_TEXT);
		}

		// Set Callbacks
		private void SetCallbacks() {
			GetNode<CheckBox>(PASSWORD_CHECKBOX).Toggled += OnPasswordCheckboxToggled;
			GetNode<LineEdit>(INPUT_GAME_NAME_TEXT).TextChanged += OnInputGameNameTextChanged;
			GetNode<LineEdit>(INPUT_GAME_NAME_TEXT).TextSubmitted += OnInputGameNameTextSubmitted;
			GetNode<LineEdit>(INPUT_PASSWORD_TEXT).TextChanged += OnInputPasswordTextChanged;
			GetNode<LineEdit>(INPUT_PASSWORD_TEXT).TextSubmitted += OnInputPasswordTextSubmitted;
			GetNode<Button>(CANCEL_BUTTON).Pressed += OnCancelButtonPressed;
			GetNode<Button>(HOST_BUTTON).Pressed += OnHostButtonPressed;
		}

		// Callbacks
		private void UpdateHostText() {
			//Implementation Here
			hostText.Text = $"Hello world!";
		}
		private void OnCancelButtonPressed() =>	CloseMenu();

		private void OnHostButtonPressed() {
			// Implementation Here
			GD.Print($"Host Button Pressed");
		}

		private void OnPasswordCheckboxToggled(bool check) {
			// Implementation Here
			GD.Print($"Password Checkbox Toggled: {check}");
		}

		private void OnInputGameNameTextChanged(string newtext) {
			// Implementation Here
			GD.Print($"New Text: {newtext}");
		}

		private void OnInputGameNameTextSubmitted(string submittedtext) {
			// Implementation Here
			GD.Print($"Submitted Text: {submittedtext}");
		}

		private void OnInputPasswordTextChanged(string newtext) {
			// Implementation Here
			GD.Print($"New Text: {newtext}");
		}

		private void OnInputPasswordTextSubmitted(string submittedtext) {
			// Implementation Here
			GD.Print($"Submitted Text: {submittedtext}");
		}

		// Set Scene Input Callbacks
		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		//Scene Input Callbacks
		public void OpenMenu() {}

		public void CloseMenu() {
			QueueFree();
		}
	}
}