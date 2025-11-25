using System;
using Core;
using Godot;

namespace Systems {
	public sealed partial class InputSystem : Node {
		public static readonly bool Debug = false;

		// Name must match action name
		public enum ActionEvent {
			MoveForward, MoveBack, MoveLeft, MoveRight,
			Jump, Sprint, Crouch,
			MenuBack, MenuExit,
			Hotbar1, Hotbar2, Hotbar3, Hotbar4, Hotbar5,
			HotbarNext, HotbarPrev,
		};

		public static readonly ActionEvent[] Actions = Enum.GetValues<ActionEvent>();

		public static event Action<ActionEvent>? OnActionPressed;
		public static event Action<ActionEvent>? OnActionReleased;

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
		private static Action<InputSystem.ActionEvent> CreateHandler(InputSystem.ActionEvent keyEvent, Action callback) {
			return (action) => {
				if(action == keyEvent) {
					callback?.Invoke();
				}
			};
		}

		public static Action WhenPressed(this InputSystem.ActionEvent keyEvent, Action callback) {
			Action<InputSystem.ActionEvent> handler = CreateHandler(keyEvent, callback);
			InputSystem.OnActionPressed += handler;
			return () => InputSystem.OnActionPressed -= handler;
		}

		public static Action WhenReleased(this InputSystem.ActionEvent keyEvent, Action callback) {
			Action<InputSystem.ActionEvent> handler = CreateHandler(keyEvent, callback);
			InputSystem.OnActionReleased += handler;
			return () => InputSystem.OnActionReleased -= handler;
		}
	}
}
