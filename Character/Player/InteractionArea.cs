using Godot;
using System;
using Components;

namespace Components {
	public partial class InteractionArea : Area3D {
		public event Action<Node3D>? OnBodyEnteredArea;
		public event Action<Node3D>? OnBodyExitedArea;

		public override void _Ready() {
			base._Ready();
			BodyEntered += HandleBodyEntered;
			BodyExited += HandleBodyExited;
		}

		private void HandleBodyEntered(Node3D body) {
			GD.Print("[InteractionArea] Body entered interaction area.");
			OnBodyEnteredArea?.Invoke(body);
		}

		private void HandleBodyExited(Node3D body) {
			GD.Print("[InteractionArea] Body exited interaction area.");
			OnBodyExitedArea?.Invoke(body);
		}
	}
}
