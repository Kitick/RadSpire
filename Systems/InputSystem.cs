using System;
using Godot;

namespace Services {
	// Name must match action name
	public enum ActionEvent {
		MoveForward, MoveBack, MoveLeft, MoveRight,
		Jump, Sprint, Crouch, Interact, Interact2, Consume, Inventory,
		MenuBack, MenuExit, MenuSelect, PageLeft, PageRight,
		MenuUp, MenuDown, MenuLeft, MenuRight,
		Hotbar1, Hotbar2, Hotbar3, Hotbar4, Hotbar5,
		HotbarNext, HotbarPrev,
	};

	public sealed partial class InputSystem : Node {
		public static readonly LogService Log = new(nameof(InputSystem), enabled: false);

		public static InputSystem Instance { get; private set; } = null!;

		public readonly ActionEvent[] Actions = Enum.GetValues<ActionEvent>();

		public event Action<ActionEvent>? OnActionPressed;
		public event Action<ActionEvent>? OnActionReleased;

		public override void _Ready() {
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
			Log.Info("Ready");
		}

		public override void _Input(InputEvent input) {
			if(input is InputEventMouseMotion mouse) {
				Log.Info($"Mouse moved {mouse.Relative}");
			}
			else if(input is InputEventJoypadMotion joypad) {
				Log.Info($"Joypad moved {joypad.Axis} : {joypad.AxisValue}");
			}
			else {
				CheckActionEvents(input);
			}
		}

		// maps our custom menu actions to Godot's built-in UI navigation actions
		private static readonly System.Collections.Generic.Dictionary<ActionEvent,string> UiMap =
			new() {
				{ ActionEvent.MenuUp, "ui_up" },
				{ ActionEvent.MenuDown, "ui_down" },
				{ ActionEvent.MenuLeft, "ui_left" },
				{ ActionEvent.MenuRight, "ui_right" },
				{ ActionEvent.MenuSelect, "ui_accept" },
				{ ActionEvent.MenuBack, "ui_cancel" },
				{ ActionEvent.MenuExit, "ui_cancel" },
			};
			
		private void CheckActionEvents(InputEvent input) {
			foreach(var action in Actions) {
				string name = action.ToString();

				if(input.IsActionPressed(name)) {
					Log.Info($"Pressed {name}");
					OnActionPressed?.Invoke(action);

					if(UiMap.TryGetValue(action, out var ui)) {
						Input.ActionPress(ui);
					}
				}
				else if(input.IsActionReleased(name)) {
					Log.Info($"Released {name}");
					OnActionReleased?.Invoke(action);

					if(UiMap.TryGetValue(action, out var ui)) {
						Input.ActionRelease(ui);
					}
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
			InputSystem.Instance.OnActionPressed += handler;
			return () => InputSystem.Instance.OnActionPressed -= handler;
		}

		public static Action WhenReleased(this ActionEvent keyEvent, Action callback) {
			Action<ActionEvent> handler = keyEvent.CreateHandler(callback);
			InputSystem.Instance.OnActionReleased += handler;
			return () => InputSystem.Instance.OnActionReleased -= handler;
		}

		public static bool IsPressed(this ActionEvent keyEvent) {
			return Input.IsActionPressed(keyEvent.ToString());
		}

		public static bool IsReleased(this ActionEvent keyEvent) {
			return !IsPressed(keyEvent);
		}
	}
}
