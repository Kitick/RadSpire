using Godot;
using System;

public partial class Hotbar : Node2D {
	
	private int selectedIndex = -1;
	private GridContainer hotbarSlots = null!;
	public override void _Ready() {
		hotbarSlots = GetNode<GridContainer>("HotbarSlots");
	}

	public override void _Input(InputEvent @event) {
		if(@event.IsActionPressed("hotbar_1")) SelectSlot(0);
		if(@event.IsActionPressed("hotbar_2")) SelectSlot(1);
		if(@event.IsActionPressed("hotbar_3")) SelectSlot(2);
		if(@event.IsActionPressed("hotbar_4")) SelectSlot(3);
		if(@event.IsActionPressed("hotbar_5")) SelectSlot(4);
	}

	private void SelectSlot(int index) {
		if(selectedIndex >= 0 && selectedIndex < hotbarSlots.GetChildCount()) {
			var prev = hotbarSlots.GetChild<Control>(selectedIndex);
			prev.Modulate = new Color(1, 1, 1);
		}

		if(index >= 0 && index < hotbarSlots.GetChildCount()) {
			var slot = hotbarSlots.GetChild<Control>(index);
			slot.Modulate = new Color(1, 1, 0);
			GD.Print($"$Hotbar slot {index + 1} selected");
			selectedIndex = index;
		}
	}
}
