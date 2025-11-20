using System;
using Core;
using Godot;

namespace Camera {
	public class CameraDrag {
		public enum DragState { Idle, Dragging, Cooldown };
		public DragState State { get; private set; } = DragState.Idle;

		public Vector3 Target { get; private set; } = Vector3.Zero;
		public Vector3 Velocity { get; private set; } = Vector3.Zero;

		public readonly Timer ResetTimer = new Timer();
		public TimeSpan ResetCooldown = TimeSpan.FromSeconds(3);

		public float Speed = 12.0f;
		public float Dampener = 8.0f;

		public CameraDrag() {
			ResetTimer.OneShot = true;
			ResetTimer.Timeout += Reset;
		}

		public void Start(Vector3 position) {
			State = DragState.Dragging;
			ResetTimer.Stop();
			Target = position;
		}

		public void Move(Vector3 delta) {
			Target += delta;
		}

		public void End() {
			State = DragState.Cooldown;
			ResetTimer.Start(ResetCooldown.Seconds);
		}

		public void Reset() {
			State = DragState.Idle;
			ResetTimer.Stop();
		}

		public void Update(Vector3 Position, float dt) {
			Vector3 VelocityTarget;

			VelocityTarget = State switch {
				DragState.Dragging => (Target - Position) * Speed,
				_ => Vector3.Zero,
			};

			Velocity = Velocity.SmoothLerp(VelocityTarget, Dampener, dt);
		}
	}
}