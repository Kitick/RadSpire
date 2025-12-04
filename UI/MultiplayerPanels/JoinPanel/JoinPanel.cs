using System;
using Godot;
using InputSystem;

namespace MultiplayerPanels {
	public partial class JoinPanel : Control {
		
		// Paths for Panel Attributes
		private const string NO_PASSWORD_CHECKBOX = "PanelArea/NoPassword/NoPasswordCheckBox";
		private const string NOT_FULL_CHECKBOX = "PanelArea/NotFull/NotFullCheckBox";
		private const string CANCEL_BUTTON = "PanelArea/CancelButton";
		private const string JOIN_BUTTON = "PanelArea/JoinButton";

		public event Action? OnMenuClosed;
		private event Action? OnExit;

		// Main
		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			SetCallbacks();
			SetInputCallbacks();
		}

		private void SetCallbacks() {
			GetNode<CheckBox>(NO_PASSWORD_CHECKBOX).Toggled += OnNoPasswordCheckboxToggled;
			GetNode<CheckBox>(NOT_FULL_CHECKBOX).Toggled += OnNotFullCheckboxToggled;
			GetNode<Button>(CANCEL_BUTTON).Pressed += OnCancelButtonPressed;
			GetNode<Button>(JOIN_BUTTON).Pressed += OnJoinButtonPressed;
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
			//Implementation Here
			GD.Print("Join Button Pressed!");
		}

		public void OpenMenu() {

		}

		public void CloseMenu() {
			QueueFree();
		}


	}
}