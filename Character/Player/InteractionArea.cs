using System;
using Godot;
using Services;

namespace Components {
	public partial class InteractionArea : Area3D {
		private static readonly LogService Log = new(nameof(InteractionArea), enabled: false);

		public event Action<Node3D>? OnBodyEnteredArea;
		public event Action<Node3D>? OnBodyExitedArea;

		public override void _Ready() {
			base._Ready();
			BodyEntered += HandleBodyEntered;
			BodyExited += HandleBodyExited;
		}

		private void HandleBodyEntered(Node3D body) {
			Log.Info("Body entered interaction area.");
			OnBodyEnteredArea?.Invoke(body);
		}

		private void HandleBodyExited(Node3D body) {
			Log.Info("Body exited interaction area.");
			OnBodyExitedArea?.Invoke(body);
		}
	}
}
