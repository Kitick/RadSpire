namespace Settings.Interface;

using System.Collections.Generic;
using Godot;
using Root;
using Services;
using Settings;

public sealed partial class KeyboardPanel : VBoxContainer {
	[Export] private HSlider MouseSenseSlider = null!;
	[Export] private CheckBox InvertedYAxisCheckBox = null!;
	[Export] private CheckBox RawInputCheckBox = null!;
	[Export] private Button RemapKeysButton = null!;

	public Control FirstControl => MouseSenseSlider;

	private static readonly Dictionary<ActionEvent, string> ActionDisplayNames = new() {
		{ ActionEvent.MoveForward,  "Move Forward" },
		{ ActionEvent.MoveBack,     "Move Back" },
		{ ActionEvent.MoveLeft,     "Move Left" },
		{ ActionEvent.MoveRight,    "Move Right" },
		{ ActionEvent.Jump,         "Jump" },
		{ ActionEvent.Sprint,       "Sprint" },
		{ ActionEvent.Crouch,       "Crouch" },
		{ ActionEvent.Dodge,        "Dodge" },
		{ ActionEvent.Block,        "Block" },
		{ ActionEvent.Attack,       "Attack" },
		{ ActionEvent.Interact,     "Interact" },
		{ ActionEvent.Enter,        "Enter" },
		{ ActionEvent.AssignNPC,    "Assign NPC" },
		{ ActionEvent.Pickup,       "Pick Up" },
		{ ActionEvent.UseItem,      "Use Item" },
		{ ActionEvent.DropItem,     "Drop Item" },
		{ ActionEvent.Place,        "Place" },
		{ ActionEvent.PlaceCancel,  "Cancel Place" },
		{ ActionEvent.BuildMode,    "Build Mode" },
		{ ActionEvent.Hotbar1,      "Hotbar 1" },
		{ ActionEvent.Hotbar2,      "Hotbar 2" },
		{ ActionEvent.Hotbar3,      "Hotbar 3" },
		{ ActionEvent.Hotbar4,      "Hotbar 4" },
		{ ActionEvent.Hotbar5,      "Hotbar 5" },
		{ ActionEvent.HotbarNext,   "Hotbar Next" },
		{ ActionEvent.HotbarPrev,   "Hotbar Prev" },
		{ ActionEvent.CameraRotate, "Rotate Camera" },
		{ ActionEvent.CameraPan,    "Pan Camera" },
		{ ActionEvent.CameraReset,  "Reset Camera" },
		{ ActionEvent.ZoomIn,       "Zoom In" },
		{ ActionEvent.ZoomOut,      "Zoom Out" },
		{ ActionEvent.Inventory,    "Inventory" },
		{ ActionEvent.QuestLog,     "Quest Log" },
		{ ActionEvent.MenuSelect,   "Menu Select" },
		{ ActionEvent.MenuExit,     "Menu Back" },
		{ ActionEvent.PageLeft,     "Page Left" },
		{ ActionEvent.PageRight,    "Page Right" },
		{ ActionEvent.DevMode,      "Dev Mode" },
	};

	public override void _Ready() {
		this.ValidateExports();
		MouseSenseSlider.ApplyBounds(KeyboardSettings.MouseSensitivity);
		RemapKeysButton.Visible = false;
		SetCallbacks();
		BuildBindingList();
	}

	private void SetCallbacks() {
		MouseSenseSlider.ValueChanged += value => KeyboardSettings.MouseSensitivity.Apply((float) value);
		InvertedYAxisCheckBox.Toggled += KeyboardSettings.InvertedYAxis.Apply;
		RawInputCheckBox.Toggled += KeyboardSettings.RawInput.Apply;
	}

	private void BuildBindingList() {
		Label heading = new() {
			Text = "Key Map",
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		heading.AddThemeFontSizeOverride("font_size", 28);
		AddChild(heading);

		ScrollContainer scroll = new() {
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
		};

		VBoxContainer list = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		scroll.AddChild(list);
		AddChild(scroll);

		foreach(ActionEvent action in ActionEvent.Actions()) {
			HBoxContainer row = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };

			Label nameLabel = new() {
				Text = ActionDisplayNames.TryGetValue(action, out string? display) ? display : action.Name,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};
			nameLabel.AddThemeFontSizeOverride("font_size", 24);

			Label keyDisplay = new() {
				Text = GetKeyLabel(action),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				CustomMinimumSize = new Vector2(200, 0),
			};
			keyDisplay.AddThemeFontSizeOverride("font_size", 24);

			row.AddChild(nameLabel);
			row.AddChild(keyDisplay);
			list.AddChild(row);
		}
	}

	private static string GetKeyLabel(ActionEvent action) {
		if(!InputMap.HasAction(action.Name)) { return "—"; }

		foreach(InputEvent ev in InputMap.ActionGetEvents(action.Name)) {
			switch(ev) {
				case InputEventKey key:
					return key.PhysicalKeycode != Key.None
						? OS.GetKeycodeString(key.PhysicalKeycode)
						: key.Keycode.ToString();
				case InputEventMouseButton mouse:
					return $"Mouse {mouse.ButtonIndex}";
				case InputEventJoypadButton joypad:
					return $"Btn {(int) joypad.ButtonIndex}";
				case InputEventJoypadMotion axis:
					return $"Axis {(int) axis.Axis} {(axis.AxisValue > 0 ? "+" : "-")}";
			}
		}

		return "—";
	}

	public void Refresh() {
		MouseSenseSlider.Value = KeyboardSettings.MouseSensitivity.Target;
		InvertedYAxisCheckBox.ButtonPressed = KeyboardSettings.InvertedYAxis.Target;
		RawInputCheckBox.ButtonPressed = KeyboardSettings.RawInput.Target;
	}
}
