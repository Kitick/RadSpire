using System;
using Core;
using Godot;

namespace Components {
	public sealed class AiInput {

		public Vector3 HorizontalInput { get; private set; }

		public bool SprintHeld { get; private set; }
		public bool CrouchHeld { get; private set; }

		public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;

		private readonly Node3D Self;
		private readonly Node3D Target;

		private readonly float SprintDistance = 7.0f;
		private readonly float StopDistance = 1.5f;

		public AiInput(Node3D self, Node3D target) {
			Self = self;
			Target = target;
		}

		public void Update() {
			HorizontalInput = Vector3.Zero;
			SprintHeld = false;
			CrouchHeld = false;

			if (Target is null) {
				return;
			}

			Vector3 toTarget = Target.GlobalPosition - Self.GlobalPosition;
			toTarget.Y = 0f;
			float dist = toTarget.Length();

			if (dist <= StopDistance) {
				// close enough → stand still (maybe attack elsewhere)
				return;
			}

			HorizontalInput = toTarget.Normalized();
			SprintHeld = dist > SprintDistance;
		}
	}
}