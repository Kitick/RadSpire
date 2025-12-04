using System;
using Godot;
using InputSystem;
using SaveSystem;

namespace LoadMenuScene {
	public sealed partial class LoadMenu : Control {
		public event Action? OnMenuClosed;
		private event Action? OnExit;

		private const string BACK_BUTTON = "BackButton";
		private const string CONTAINER = "Panel/SaveSlots";

		public static string SlotFile(int slot) => $"slot{slot}";

		private const int SLOTS = 5;

		private Button[] Buttons = new Button[SLOTS];

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;

			SetInputCallbacks();
			GetComponents();
			SetCallbacks();
		}

		public override void _ExitTree() {
			OnExit?.Invoke();
			OnMenuClosed?.Invoke();
		}

		private void SetInputCallbacks() {
			OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
			OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
		}

		private void GetComponents() {
			for(int i = 0; i < SLOTS; i++) {
				int slot = i + 1;

				bool exists = SaveService.Exists(SlotFile(slot));

				Buttons[i] = GetNode<Button>($"{CONTAINER}/Slot{slot}");

				Buttons[i].Text = exists ? $"Slot {slot}" : "Empty";
				Buttons[i].Pressed += () => OnLoadSlotPressed(slot);
			}
		}

		private void SetCallbacks() {
			GetNode<Button>(BACK_BUTTON).Pressed += OnBackButtonPressed;
		}

		private void OnBackButtonPressed() {
			CloseMenu();
		}

		public void OpenMenu() {

		}

		private void CloseMenu() {
			QueueFree();
		}

		private void OnLoadSlotPressed(int slot) {

		}
	}
}