using System;
using Godot;

namespace InputSystem {
	// Name must match action name
	public enum ActionEvent {
		MoveForward, MoveBack, MoveLeft, MoveRight,
		Jump, Sprint, Crouch,
		MenuBack, MenuExit,
		Hotbar1, Hotbar2, Hotbar3, Hotbar4, Hotbar5,
		HotbarNext, HotbarPrev,
	};

	public sealed partial class InputSystem : Node {
		public static readonly bool Debug = false;

		public static readonly ActionEvent[] Actions = Enum.GetValues<ActionEvent>();

		public static event Action<ActionEvent>? OnActionPressed;
		public static event Action<ActionEvent>? OnActionReleased;

		public override void _Ready() {
			Log("Ready");
			ProcessMode = ProcessModeEnum.Always;
		}

		public override void _Input(InputEvent input) {
			if(input is InputEventMouseMotion mouse) {
				Log($"Mouse moved {mouse.Relative}");
			}
			else if(input is InputEventJoypadMotion joypad) {
				Log($"Joypad moved {joypad.Axis} : {joypad.AxisValue}");
			}
			else {
				CheckActionEvents(input);
			}
		}

		private static void Log(string message) {
			if(Debug) {
				GD.Print($"[InputSystem] {message}");
			}
		}

		private static void CheckActionEvents(InputEvent input) {
			foreach(var action in Actions) {
				string name = action.ToString();

				if(input.IsActionPressed(name)) {
					Log($"Pressed {name}");
					OnActionPressed?.Invoke(action);

				}
				else if(input.IsActionReleased(name)) {
					Log($"Released {name}");
					OnActionReleased?.Invoke(action);
				}
			}
		}
	}

	public static class InputSystemExtensions {
		private static Action<ActionEvent> CreateHandler(this ActionEvent keyEvent, Action callback) {
			return (action) => {
				if(action == keyEvent) {
					callback?.Invoke();
				}
			};
		}

		public static Action WhenPressed(this ActionEvent keyEvent, Action callback) {
			Action<ActionEvent> handler = keyEvent.CreateHandler(callback);
			InputSystem.OnActionPressed += handler;
			return () => InputSystem.OnActionPressed -= handler;
		}

		public static Action WhenReleased(this ActionEvent keyEvent, Action callback) {
			Action<ActionEvent> handler = keyEvent.CreateHandler(callback);
			InputSystem.OnActionReleased += handler;
			return () => InputSystem.OnActionReleased -= handler;
		}

		public static bool IsPressed(this ActionEvent keyEvent) {
			return Input.IsActionPressed(keyEvent.ToString());
		}

		public static bool IsReleased(this ActionEvent keyEvent) {
			return !IsPressed(keyEvent);
		}
	}
}
