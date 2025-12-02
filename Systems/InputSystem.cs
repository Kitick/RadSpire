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
			if(Debug) { GD.Print("InputSystem: Ready"); }
			ProcessMode = ProcessModeEnum.Always;
		}

		private static void CheckActionEvents(InputEvent input) {
			foreach(var action in Actions) {
				string name = action.ToString();

				if(input.IsActionPressed(name)) {
					if(Debug) { GD.Print($"InputSystem: Pressed {name}"); }
					OnActionPressed?.Invoke(action);

				}
				else if(input.IsActionReleased(name)) {
					if(Debug) { GD.Print($"InputSystem: Released {name}"); }
					OnActionReleased?.Invoke(action);
				}
			}
		}

		public override void _Input(InputEvent input) {
			if(input is InputEventMouseMotion mouse) {
				if(Debug) { GD.Print($"InputSystem: Mouse moved {mouse.Relative}"); }
			}
			else if(input is InputEventJoypadMotion joypad) {
				if(Debug) { GD.Print($"InputSystem: Joypad moved {joypad.Axis} : {joypad.AxisValue}"); }
			}
			else {
				CheckActionEvents(input);
			}
		}
	}

	public static class InputSystemExtensions {
		private static Action<ActionEvent> CreateHandler(ActionEvent keyEvent, Action callback) {
			return (action) => {
				if(action == keyEvent) {
					callback?.Invoke();
				}
			};
		}

		public static Action WhenPressed(this ActionEvent keyEvent, Action callback) {
			Action<ActionEvent> handler = CreateHandler(keyEvent, callback);
			InputSystem.OnActionPressed += handler;
			return () => InputSystem.OnActionPressed -= handler;
		}

		public static Action WhenReleased(this ActionEvent keyEvent, Action callback) {
			Action<ActionEvent> handler = CreateHandler(keyEvent, callback);
			InputSystem.OnActionReleased += handler;
			return () => InputSystem.OnActionReleased -= handler;
		}
	}
}
