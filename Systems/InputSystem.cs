namespace Services;

using System;
using System.Collections.Generic;
using Godot;

public sealed partial class InputSystem : Node {
	public static readonly LogService Log = new(nameof(InputSystem), enabled: false);

	public static InputSystem Instance { get; private set; } = null!;

	public event Action<ActionEvent>? OnActionPressed;
	public event Action<ActionEvent>? OnActionReleased;

	public event Action<InputEventMouseMotion>? OnMouseMoved;
	public event Action<InputEventJoypadMotion>? OnJoypadMoved;

	public enum InputMode { MouseKeyboard, Controller }
	public InputMode CurrentInputMode { get; private set; } = InputMode.MouseKeyboard;
	public event Action<InputMode>? OnInputModeChanged;

	private const float ControllerModeDeadzoneDefault = 0.2f;

	public override void _Ready() {
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;
		Log.Info("Ready");
	}

	public override void _Input(InputEvent input) {
		if(input is InputEventMouseMotion mouse) {
			SetInputMode(InputMode.MouseKeyboard);
			Log.Info($"Mouse moved {mouse.Relative}");
			OnMouseMoved?.Invoke(mouse);
		}
		else if(input is InputEventJoypadMotion joypad) {
			if(Mathf.Abs(joypad.AxisValue) > ControllerModeDeadzoneDefault) { SetInputMode(InputMode.Controller); }
			Log.Info($"Joypad moved {joypad.Axis} : {joypad.AxisValue}");
			OnJoypadMoved?.Invoke(joypad);
		}
		else {
			if(input is InputEventKey or InputEventMouseButton) { SetInputMode(InputMode.MouseKeyboard); }
			else if(input is InputEventJoypadButton) { SetInputMode(InputMode.Controller); }
			CheckActionEvents(input);
		}
	}

	private void SetInputMode(InputMode mode) {
		if(CurrentInputMode == mode) { return; }
		CurrentInputMode = mode;
		Log.Info($"Input mode changed: {mode}");
		OnInputModeChanged?.Invoke(mode);
	}

	private void CheckActionEvents(InputEvent input) {
		foreach(ActionEvent action in ActionEvent.Actions()) {
			if(!InputMap.HasAction(action.Name)) {
				continue;
			}
			if(input.IsActionPressed(action.Name)) {
				Log.Info($"Pressed {action.Name}");
				OnActionPressed?.Invoke(action);
			}
			else if(input.IsActionReleased(action.Name)) {
				Log.Info($"Released {action.Name}");
				OnActionReleased?.Invoke(action);
			}
		}
	}
}

public static class ActionEventExtensions {
	private static Action<ActionEvent> CreateHandler(this ActionEvent keyEvent, Action callback) {
		return (action) => {
			if(action.Name == keyEvent.Name) { callback?.Invoke(); }
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
		return Input.IsActionPressed(keyEvent.Name);
	}

	public static bool IsJustPressed(this ActionEvent keyEvent) {
		return Input.IsActionJustPressed(keyEvent.Name);
	}

	public static bool IsReleased(this ActionEvent keyEvent) {
		return !IsPressed(keyEvent);
	}
}

public readonly struct ActionEvent {
	public readonly StringName Name;
	private ActionEvent(StringName name) => Name = name;

	// Movement
	public static readonly ActionEvent MoveForward = new("MoveForward");
	public static readonly ActionEvent MoveBack = new("MoveBack");
	public static readonly ActionEvent MoveLeft = new("MoveLeft");
	public static readonly ActionEvent MoveRight = new("MoveRight");
	public static readonly ActionEvent Jump = new("Jump");
	public static readonly ActionEvent Sprint = new("Sprint");
	public static readonly ActionEvent Crouch = new("Crouch");
	public static readonly ActionEvent Dodge = new("Dodge");

	// Combat
	public static readonly ActionEvent Attack = new("Attack");

	// Interaction
	public static readonly ActionEvent Interact = new("Interact");
	public static readonly ActionEvent AssignNPC = new("AssignNPC");
	public static readonly ActionEvent Pickup = new("Pickup");
	public static readonly ActionEvent UseItem = new("UseItem");
	public static readonly ActionEvent DropItem = new("DropItem");

	// Building
	public static readonly ActionEvent Place = new("Place");
	public static readonly ActionEvent PlaceCancel = new("PlaceCancel");
	public static readonly ActionEvent BuildMode = new("BuildMode");

	// Hotbar
	public static readonly ActionEvent Hotbar1 = new("Hotbar1");
	public static readonly ActionEvent Hotbar2 = new("Hotbar2");
	public static readonly ActionEvent Hotbar3 = new("Hotbar3");
	public static readonly ActionEvent Hotbar4 = new("Hotbar4");
	public static readonly ActionEvent Hotbar5 = new("Hotbar5");
	public static readonly ActionEvent HotbarNext = new("HotbarNext");
	public static readonly ActionEvent HotbarPrev = new("HotbarPrev");

	// Camera
	public static readonly ActionEvent CameraRotate = new("RotateCamera");
	public static readonly ActionEvent CameraPan = new("PanCamera");
	public static readonly ActionEvent CameraReset = new("ResetCamera");
	public static readonly ActionEvent ZoomIn = new("ZoomIn");
	public static readonly ActionEvent ZoomOut = new("ZoomOut");

	// UI
	public static readonly ActionEvent Inventory = new("Inventory");
	public static readonly ActionEvent QuestLog = new("QuestLog");
	public static readonly ActionEvent MenuSelect = new("ui_accept");
	public static readonly ActionEvent MenuExit = new("ui_cancel");
	public static readonly ActionEvent PageLeft = new("PageLeft");
	public static readonly ActionEvent PageRight = new("PageRight");

	// Dev
	public static readonly ActionEvent DevMode = new("DevMode");

	public static IEnumerable<ActionEvent> Actions() {
		// Movement
		yield return MoveForward;
		yield return MoveBack;
		yield return MoveLeft;
		yield return MoveRight;
		yield return Jump;
		yield return Sprint;
		yield return Crouch;
		yield return Dodge;

		// Combat
		yield return Attack;

		// Interaction
		yield return Interact;
		yield return AssignNPC;
		yield return Pickup;
		yield return UseItem;
		yield return DropItem;

		// Building
		yield return Place;
		yield return PlaceCancel;
		yield return BuildMode;

		// Hotbar
		yield return Hotbar1;
		yield return Hotbar2;
		yield return Hotbar3;
		yield return Hotbar4;
		yield return Hotbar5;
		yield return HotbarNext;
		yield return HotbarPrev;

		// Camera
		yield return CameraRotate;
		yield return CameraPan;
		yield return CameraReset;
		yield return ZoomIn;
		yield return ZoomOut;

		// UI
		yield return Inventory;
		yield return QuestLog;
		yield return MenuSelect;
		yield return MenuExit;
		yield return PageLeft;
		yield return PageRight;

		// Dev
		yield return DevMode;
	}
}
