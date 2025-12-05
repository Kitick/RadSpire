using System;
using Components;
using Core;
using SaveSystem;
using Godot;

public partial class ItemIconTriggerArea : Area3D {

	private static readonly Logger Log = new(nameof(ItemIconTriggerArea), enabled: false);

	public event Action<Item3DIcon>? OnPlayerEnteredItemIconRange;
	public event Action<Item3DIcon>? OnPlayerExitedItemIconRange;

	public Item3DIcon item3DIcon = null!;

	public override void _Ready() {
		base._Ready();
		BodyEntered += HandleBodyEntered;
		BodyExited += HandleBodyExited;
		item3DIcon = GetNode<Item3DIcon>("../../..");
	}

	private void HandleBodyEntered(Node3D body) {
		if (body is Player) {
			OnPlayerEnteredItemIconRange?.Invoke(item3DIcon);
			Log.Info("Player entered item icon trigger area.");
		}
	}

	private void HandleBodyExited(Node3D body) {
		if (body is Player) {
			OnPlayerExitedItemIconRange?.Invoke(item3DIcon);
			Log.Info("Player exited item icon trigger area.");
		}
	}
}
