using System;
using Core;
using Godot;

namespace Components {
	public class KeyInput {
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

		private static Vector3 GetHorizontalMovement() {
			Vector3 direction = Vector3.Zero;

			if(Input.IsActionPressed(Actions.Forward)) { direction += Vector3.Forward; }
			if(Input.IsActionPressed(Actions.Back)) { direction += Vector3.Back; }
			if(Input.IsActionPressed(Actions.Left)) { direction += Vector3.Left; }
			if(Input.IsActionPressed(Actions.Right)) { direction += Vector3.Right; }

			return direction.Normalized();
		}
	}
}
