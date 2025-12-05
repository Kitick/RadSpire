using System;
using Godot;
using InputSystem;
using Network;

namespace MultiplayerPanels {
	public sealed partial class HostPanel : Control {
		private static readonly Logger Log = new(nameof(HostPanel), enabled: true);

		//Paths For Panel Attributes
		private const string PANEL_AREA = "PanelArea";
		private const string LABEL_HOST_TEXT = PANEL_AREA + "/lblHostText";
		private const string PASSWORD_CHECKBOX = PANEL_AREA + "/PasswordContainer/lblPassword/PasswordCheckbox";
		private const string INPUT_GAME_NAME_TEXT = PANEL_AREA + "/GameNameContainer/InputGameName";
		private const string INPUT_PASSWORD_TEXT = PANEL_AREA + "/PasswordContainer/InputPassword";
		private const string CANCEL_BUTTON = PANEL_AREA + "/OptionContainer/CancelButton";
		private const string HOST_BUTTON = PANEL_AREA + "/OptionContainer/HostButton";

		//Component Reference
		public Label HostText = null!;
		public LineEdit InputGameName = null!;
		public LineEdit InputPassword = null!;

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
			HostText = GetNode<Label>(LABEL_HOST_TEXT);
			InputGameName = GetNode<LineEdit>(INPUT_GAME_NAME_TEXT);
			InputPassword = GetNode<LineEdit>(INPUT_PASSWORD_TEXT);
		}

		// Set Callbacks
		private void SetCallbacks() {
			GetNode<CheckBox>(PASSWORD_CHECKBOX).Toggled += OnPasswordCheckboxToggled;
			InputGameName.TextChanged += OnInputGameNameTextChanged;
			InputPassword.TextChanged += OnInputPasswordTextChanged;
			InputGameName.Connect("text_submitted", Callable.From<string>(text => OnAnyTextSubmitted(INPUT_GAME_NAME_TEXT, text)));
			InputPassword.Connect("text_submitted", Callable.From<string>(text => OnAnyTextSubmitted(INPUT_PASSWORD_TEXT, text)));
			GetNode<Button>(CANCEL_BUTTON).Pressed += OnCancelButtonPressed;
			GetNode<Button>(HOST_BUTTON).Pressed += OnHostButtonPressed;
		}

		// Callbacks
		public void UpdateHostText(string newText) {
			HostText.Text = newText;
		}

		private void OnCancelButtonPressed() => CloseMenu();

		private void OnHostButtonPressed() {
			Error result = Server.Instance.Host();
			if(result == Error.Ok) {
				Log.Info("Successfully started hosting");
				// Only start a new game if not already in game
				if(!GameManager.Instance.InGame) {
					GameManager.Instance.StartNewGame();
				}
				CloseMenu();
			}
			else {
				Log.Error($"Failed to host: {result}");
				UpdateHostText($"Failed to host: {result}");
			}
		}
		private void OnPasswordCheckboxToggled(bool check) {
			if(check == true) {
				InputPassword.Show();
			}
			else if(check == false) {
				InputPassword.Hide();
			}
		}

		// User Text Input
		private void OnInputGameNameTextChanged(string newtext) {
			// Implementation Here
			Log.Info($"Game Name New Text: {newtext}");
		}

		private void OnInputPasswordTextChanged(string newtext) {
			// Implementation Here
			Log.Info($"Password New Text: {newtext}");
		}

		// User Text Submission
		private void OnAnyTextSubmitted(string sourceName, string submittedText) {
			if(sourceName == null) return;

			if(sourceName == INPUT_GAME_NAME_TEXT) {
				Log.Info($"Game Name Submitted: {submittedText}");
			}

			else if(sourceName == INPUT_PASSWORD_TEXT) {
				Log.Info($"Password Submitted: {submittedText}");
			}
		}

		// Set Scene Input Callbacks
		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		//Scene Input Callbacks
		public void OpenMenu() { }

		public void CloseMenu() {
			QueueFree();
		}
	}
}