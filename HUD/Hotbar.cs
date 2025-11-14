using System;
using Constants;
using Godot;

public partial class Hotbar : Node2D {
	private int selectedIndex = -1;
	private GridContainer hotbarSlots = null!;

	private const string HOTBAR = "HotbarSlots";

	private static readonly Color TintColor = new Color(1f, 1f, 0.3f);

	public override void _Ready() {
		hotbarSlots = GetNode<GridContainer>(HOTBAR);
	}

	public override void _Input(InputEvent input) {
		if(input.IsActionPressed(Actions.Hotbar1)) { SelectSlot(0); }
		if(input.IsActionPressed(Actions.Hotbar2)) { SelectSlot(1); }
		if(input.IsActionPressed(Actions.Hotbar3)) { SelectSlot(2); }
		if(input.IsActionPressed(Actions.Hotbar4)) { SelectSlot(3); }
		if(input.IsActionPressed(Actions.Hotbar5)) { SelectSlot(4); }
	}

	private void SelectSlot(int index) {
		// Reset previous slot
		if(selectedIndex >= 0 && selectedIndex < hotbarSlots.GetChildCount()) {
			var prev = hotbarSlots.GetChild<Panel>(selectedIndex);
			prev.SelfModulate = Colors.White;
			prev.Scale = Vector2.One;
		}

		// Highlight new slot
		if(index >= 0 && index < hotbarSlots.GetChildCount()) {
			var slot = hotbarSlots.GetChild<Panel>(index);
			slot.SelfModulate = TintColor;

			// with slight enlargement
			slot.Scale = new Vector2(1.05f, 1.05f);

			selectedIndex = index;

			GD.Print($"Hotbar slot {index + 1} selected");
		}
	}
}
