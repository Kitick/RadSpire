namespace Character;

using Camera;
using Godot;
using Root;
using Services;

public sealed class KeyInput {
	public Vector3 HorizontalInput { get; private set; }

	public bool SprintHeld { get; private set; }
	public bool CrouchHeld { get; private set; }
	public bool JumpPressed { get; private set; }
	public bool AttackPressed { get; private set; }

	public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;

	public void Update(CameraRig camera) {
		HorizontalInput = GetHorizontalMovement(camera);
		JumpPressed = ActionEvent.Jump.IsJustPressed();
		SprintHeld = ActionEvent.Sprint.IsPressed();
		CrouchHeld = ActionEvent.Crouch.IsPressed();
		AttackPressed = ActionEvent.Attack.IsJustPressed();
	}

	private static Vector3 GetHorizontalMovement(CameraRig camera) {
		Vector2 inputVector = Input.GetVector(ActionEvent.MoveLeft.Name, ActionEvent.MoveRight.Name, ActionEvent.MoveForward.Name, ActionEvent.MoveBack.Name);

		Vector3 direction = new Vector3(inputVector.X, 0, inputVector.Y);

		Vector3 rotated = camera.Pose.AlignVector(direction);

		return rotated.Normalized();
	}
}
