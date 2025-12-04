using System;
using Godot;
using InputSystem;

namespace LoadMenuScene {
	public sealed partial class LoadMenu : Control {
		public event Action? OnMenuClosed;
		private event Action? OnExit;

		private const string BACK_BUTTON = "BackButton";
		private const string CONTAINER = "VLoadContainer";

		private Button[] loadButtons = null!;
		private Label[] infoLabels = null!;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			GetComponents();
			SetCallbacks();
			SetInputCallbacks();
		}

		private void GetComponents() {
			loadButtons = new Button[5];
			infoLabels = new Label[5];

			for(int i = 0; i < 5; i++) {
				loadButtons[i] = GetNode<Button>($"{CONTAINER}/Load{i + 1}");
				infoLabels[i] = GetNode<Label>($"{CONTAINER}/Load{i + 1}/InfoText{i + 1}");

				int slotIndex = i;
				loadButtons[i].Pressed += () => OnLoadSlotPressed(slotIndex);
			}

			LoadSavedGames();
		}

		private void SetCallbacks() {
			GetNode<Button>(BACK_BUTTON).Pressed += OnBackButtonPressed;
		}

		private void OnBackButtonPressed() {
			CloseMenu();
		}

		private void LoadSavedGames() {
			//Implementation Here
		}

		private void OnLoadSlotPressed(int index) {
			//Implementation Here
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		public void OpenMenu() {

		}

		private void CloseMenu() {
			QueueFree();
		}
	}
}