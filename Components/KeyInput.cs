namespace Components {
	using Camera;
	using Core;
	using Godot;

	public sealed class KeyInput {
		public Vector3 HorizontalInput { get; private set; }

		public bool SprintHeld { get; private set; }
		public bool CrouchHeld { get; private set; }
		public bool JumpPressed { get; private set; }
		public bool AttackPressed { get; private set; }

		public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;

		public void Update(CameraRig camera) {
			HorizontalInput = GetHorizontalMovement(camera);
			JumpPressed = Input.IsActionJustPressed(Actions.Jump);
			SprintHeld = Input.IsActionPressed(Actions.Sprint);
			CrouchHeld = Input.IsActionPressed(Actions.Crouch);
			AttackPressed = Input.IsActionPressed(Actions.Attack);
		}

		private static Vector3 GetHorizontalMovement(CameraRig camera) {
			Vector2 inputVector = Input.GetVector(Actions.MoveLeft, Actions.MoveRight, Actions.MoveForward, Actions.MoveBack);

			Vector3 direction = new Vector3(inputVector.X, 0, inputVector.Y);

			Vector3 rotated = camera.Pose.AlignVector(direction);

			return rotated.Normalized();
		}
	}
}
