using System;
using Camera;
using Core;
using Godot;

namespace Components {
	public sealed class KeyInput {
		public CameraRig? Camera;

		public Vector3 HorizontalInput { get; private set; }

		public bool SprintHeld { get; private set; }
		public bool CrouchHeld { get; private set; }

		public bool JumpPressed { get; private set; }

		public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;

		public void Update() {
			HorizontalInput = GetHorizontalMovement();
			JumpPressed = Input.IsActionJustPressed(Actions.Jump);
			SprintHeld = Input.IsActionPressed(Actions.Sprint);
			CrouchHeld = Input.IsActionPressed(Actions.Crouch);
		}

		private Vector3 GetHorizontalMovement() {
			Vector2 inputVector = Input.GetVector(Actions.MoveLeft, Actions.MoveRight, Actions.MoveForward, Actions.MoveBack);

			Vector3 direction = new Vector3(inputVector.X, 0, inputVector.Y);

			float hdg = Camera?.Pose.RadHDG ?? 0;

			Vector3 rotated = direction.Rotated(Vector3.Up, hdg);

			return rotated.Normalized();
		}
	}
}
