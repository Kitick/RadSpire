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
		public static Label hostText = null!;
		public static LineEdit inputGameName = null!;
		public static LineEdit inputPassword = null!;

		// Events
		public event Action? OnMenuClosed;
		private event Action? OnExit;

		// Override Functions
		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			GetComponents();

			SetCallbacks();
			SetInputCallbacks();
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		//Get Components
		private void GetComponents() {
			hostText = GetNode<Label>(LABEL_HOST_TEXT);
			inputGameName = GetNode<LineEdit>(INPUT_GAME_NAME_TEXT);
			inputPassword = GetNode<LineEdit>(INPUT_PASSWORD_TEXT);
		}

		// Set Callbacks
		private void SetCallbacks() {
			GetNode<CheckBox>(PASSWORD_CHECKBOX).Toggled += OnPasswordCheckboxToggled;
			inputGameName.TextChanged += OnInputGameNameTextChanged;
			inputPassword.TextChanged += OnInputPasswordTextChanged;
			inputGameName.Connect("text_submitted", Callable.From<string>(text => OnAnyTextSubmitted(INPUT_GAME_NAME_TEXT, text)));
			inputPassword.Connect("text_submitted", Callable.From<string>(text => OnAnyTextSubmitted(INPUT_PASSWORD_TEXT, text)));
			GetNode<Button>(CANCEL_BUTTON).Pressed += OnCancelButtonPressed;
			GetNode<Button>(HOST_BUTTON).Pressed += OnHostButtonPressed;
		}

		// Callbacks
		public void UpdateHostText(string newText) {
			hostText.Text = newText;
		}
		private void OnCancelButtonPressed() =>	CloseMenu();

		private void OnHostButtonPressed() {
			// Implementation Here
			GD.Print($"Host Button Pressed");
		}

		private void OnPasswordCheckboxToggled(bool check) {	
			if(check == true) {
				inputPassword.Show();
			}
			else if(check == false) {
				inputPassword.Hide();
			}
		}

		// User Text Input
		private void OnInputGameNameTextChanged(string newtext) {
			// Implementation Here
			GD.Print($"Game Name New Text: {newtext}");
		}

		private void OnInputPasswordTextChanged(string newtext) {
			// Implementation Here
			GD.Print($"Password New Text: {newtext}");
		}

		// User Text Submission
		private void OnAnyTextSubmitted(string sourceName, string submittedText) {
			if(sourceName == null) return;

			if(sourceName == INPUT_GAME_NAME_TEXT) {
				GD.Print($"Game Name Submitted: {submittedText}");
			}

			else if(sourceName == INPUT_PASSWORD_TEXT) {
				GD.Print($"Password Submitted: {submittedText}");
			}
		}

		// Set Scene Input Callbacks
		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		//Scene Input Callbacks
		public void OpenMenu(){}

		public void CloseMenu() {
			QueueFree();
		}
	}
}