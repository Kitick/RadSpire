namespace Components;

using System;
using Godot;
using Services;

public partial class InteractionArea : Area3D {
	private static readonly LogService Log = new(nameof(InteractionArea), enabled: false);

	public event Action<Node3D>? OnBodyEnteredArea;
	public event Action<Node3D>? OnBodyExitedArea;
	public event Action<Area3D>? OnAreaEnteredArea;
	public event Action<Area3D>? OnAreaExitedArea;

	public override void _Ready() {
		base._Ready();
		BodyEntered += HandleBodyEntered;
		BodyExited += HandleBodyExited;
		AreaEntered += HandleAreaEntered;
		AreaExited += HandleAreaExited;
	}

	private void HandleBodyEntered(Node3D body) {
		Log.Info("Body entered interaction area.");
		OnBodyEnteredArea?.Invoke(body);
	}

	private void HandleBodyExited(Node3D body) {
		Log.Info("Body exited interaction area.");
		OnBodyExitedArea?.Invoke(body);
	}

	private void HandleAreaEntered(Area3D area) {
		Log.Info("Area entered interaction area.");
		OnAreaEnteredArea?.Invoke(area);
	}

	private void HandleAreaExited(Area3D area) {
		Log.Info("Area exited interaction area.");
		OnAreaExitedArea?.Invoke(area);
	}
}
