namespace QuestSystem;

using System;
using Godot;
using Root;
using Services;

public sealed partial class QuestLocationTrigger : Area3D {
	private static readonly LogService Log = new(nameof(QuestLocationTrigger), enabled: true);

	[Export] public LocationID Location { get; set; } = LocationID.None;

	public event Action<LocationID>? PlayerReachedLocation;

	private event Action? OnExit;
	private bool Fired = false;

	public override void _Ready() {
		if(Location == LocationID.None) {
			Log.Error($"{Name}: Location not assigned.");
			return;
		}
		BodyEntered += HandleBodyEntered;
	}

	private void HandleBodyEntered(Node3D body) {
		if(Fired) { return; }
		if(!body.IsInGroup(Group.Player.ToString())) { return; }
		Fired = true;
		Log.Info($"Player reached location '{Location}'");
		PlayerReachedLocation?.Invoke(Location);
	}

	public override void _ExitTree() {
		OnExit?.Invoke();
		ClearEvents();
	}

	private void ClearEvents() {
		PlayerReachedLocation = null;
	}
}
