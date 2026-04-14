namespace QuestSystem;

using System;
using Godot;
using Root;
using Services;

public sealed partial class QuestLocationTrigger : Area3D {
	private static readonly LogService Log = new(nameof(QuestLocationTrigger), enabled: true);

	[Export] public string LocationId { get; set; } = "";

	public event Action<string>? PlayerReachedLocation;

	private bool Fired = false;

	public override void _Ready() {
		this.ValidateExports();
		BodyEntered += HandleBodyEntered;
	}

	private void HandleBodyEntered(Node3D body) {
		if(Fired) { return; }
		if(!body.IsInGroup("player")) { return; }
		if(string.IsNullOrWhiteSpace(LocationId)) {
			Log.Error("QuestLocationTrigger: LocationId is empty — trigger will not fire.");
			return;
		}
		Fired = true;
		Log.Info($"Player reached location '{LocationId}'");
		PlayerReachedLocation?.Invoke(LocationId);
	}
}
