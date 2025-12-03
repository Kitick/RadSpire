using System;
using Godot;
using InputSystem;

namespace MultiplayerPanels {
	public sealed partial class HostPanel : Control {
		public event Action? OnMenuClosed;
		private event Action? OnExit;

		public override void _Ready() {
			ProcessMode = ProcessModeEnum.Always;
			SetInputCallbacks();
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

		public void CloseMenu() {
			QueueFree();
		}
	}
}