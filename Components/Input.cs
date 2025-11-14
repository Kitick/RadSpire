using System;
using Godot;

namespace Components {
	public class KeyInput {
		private const string JUMP = "jump";
		private const string SPRINT = "sprint";
		private const string CROUCH = "crouch";

		private const string FORWARD = "move_forward";
		private const string BACK = "move_back";
		private const string LEFT = "move_left";
		private const string RIGHT = "move_right";

		private const float EPSILON = 0.01f;

		public Vector3 HorizontalInput { get; private set; }

		public bool SprintHeld { get; private set; }
		public bool CrouchHeld { get; private set; }

		public bool JumpPressed { get; private set; }

		public bool IsMoving => HorizontalInput.Length() >= EPSILON;

		public void Update() {
			HorizontalInput = GetHorizontalMovement();
			JumpPressed = Input.IsActionJustPressed(JUMP);
			SprintHeld = Input.IsActionPressed(SPRINT);
			CrouchHeld = Input.IsActionPressed(CROUCH);
		}

		private static Vector3 GetHorizontalMovement() {
			Vector3 direction = Vector3.Zero;

			if(Input.IsActionPressed(FORWARD)) { direction += Vector3.Forward; }
			if(Input.IsActionPressed(BACK)) { direction += Vector3.Back; }
			if(Input.IsActionPressed(RIGHT)) { direction += Vector3.Right; }
			if(Input.IsActionPressed(LEFT)) { direction += Vector3.Left; }

			return direction.Normalized();
		}
	}
}
