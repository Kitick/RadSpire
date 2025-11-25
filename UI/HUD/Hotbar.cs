using System;
using System.Collections.Generic;
using Godot;
using Systems;

public sealed partial class Hotbar : Control {
	public static readonly bool Debug = false;

	public int SelectedSlot {
		get;
		set {
			value += HotbarSlots.Count;
			value %= HotbarSlots.Count;
			field = value;
			SelectSlot(HotbarSlots[value]);
		}
	}

	private readonly List<Panel> HotbarSlots = [];

	private static readonly Color NormalColor = Colors.White;
	private static readonly Color SelectColor = new Color(1f, 1f, 0.3f);
	private static readonly Vector2 SelectedScale = new Vector2(1.05f, 1.05f);

	private const string HOTBAR = "Background/GridBackground/HotbarSlots";

	private event Action? OnExit;

	public override void _EnterTree() {
		SetInputCallbacks();
		RequestReady();
	}

	public override void _Ready() {
		GetComponents();
		SelectedSlot = 0;
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
	}

	private void GetComponents() {
		var container = GetNode<GridContainer>(HOTBAR);

		foreach(var child in container.GetChildren()) {
			if(child is Panel slot) {
				HotbarSlots.Add(slot);
			}
		}
	}

	private void SetInputCallbacks() {
		OnExit += InputSystem.ActionEvent.Hotbar1.WhenPressed(() => SelectedSlot = 0);
		OnExit += InputSystem.ActionEvent.Hotbar2.WhenPressed(() => SelectedSlot = 1);
		OnExit += InputSystem.ActionEvent.Hotbar3.WhenPressed(() => SelectedSlot = 2);
		OnExit += InputSystem.ActionEvent.Hotbar4.WhenPressed(() => SelectedSlot = 3);
		OnExit += InputSystem.ActionEvent.Hotbar5.WhenPressed(() => SelectedSlot = 4);

		OnExit += InputSystem.ActionEvent.HotbarNext.WhenPressed(() => SelectedSlot++);
		OnExit += InputSystem.ActionEvent.HotbarPrev.WhenPressed(() => SelectedSlot--);
	}

	private void SelectSlot(Panel slot) {
		if(Debug) { GD.Print($"Hotbar: Selecting slot {HotbarSlots.IndexOf(slot)}"); }

		foreach(var other in HotbarSlots) {
			bool selected = other == slot;

			other.SelfModulate = selected ? SelectColor : NormalColor;
			other.Scale = selected ? SelectedScale : Vector2.One;
		}
	}
}
