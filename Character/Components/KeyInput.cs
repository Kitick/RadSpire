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
	public bool DodgePressed { get; private set; }
	public bool BlockPressed { get; private set; }
	public bool BlockHeld { get; private set; }

	public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;
	private bool AltHeldPrev = false;
	private bool BlockHeldPrev = false;

	public void Update(CameraRig camera) {
		HorizontalInput = GetHorizontalMovement(camera);
		JumpPressed = ActionEvent.Jump.IsJustPressed();
		SprintHeld = ActionEvent.Sprint.IsPressed();
		CrouchHeld = ActionEvent.Crouch.IsPressed();
		bool hoveringInteractive = IsHoveringInteractiveControl(camera.GetViewport());
		AttackPressed = ActionEvent.Attack.IsJustPressed() && !hoveringInteractive;
		bool blockHeldRaw = Input.IsKeyPressed(Key.Capslock);
		BlockHeld = blockHeldRaw && !hoveringInteractive;
		BlockPressed = BlockHeld && !BlockHeldPrev;
		BlockHeldPrev = BlockHeld;

		bool dodgeHeld = ActionEvent.Dodge.IsPressed();
		bool dodgeJust = ActionEvent.Dodge.IsJustPressed();
		bool altHeld = Input.IsKeyPressed(Key.Alt);
		bool altJust = altHeld && !AltHeldPrev;
		bool moveJust =
			Input.IsActionJustPressed(ActionEvent.MoveLeft.Name) ||
			Input.IsActionJustPressed(ActionEvent.MoveRight.Name) ||
			Input.IsActionJustPressed(ActionEvent.MoveForward.Name) ||
			Input.IsActionJustPressed(ActionEvent.MoveBack.Name);

		// Allow Alt+Move regardless of press order.
		DodgePressed = (dodgeJust && IsMoving) || (dodgeHeld && moveJust) || (altJust && IsMoving) || (altHeld && moveJust);
		AltHeldPrev = altHeld;
	}

	private static bool IsHoveringInteractiveControl(Viewport viewport) {
		Control? hovered = viewport.GuiGetHoveredControl();
		return hovered is Button or BaseButton or Slider or SpinBox or LineEdit or TextEdit or OptionButton or MenuButton;
	}

	private static Vector3 GetHorizontalMovement(CameraRig camera) {
		Vector2 inputVector = Input.GetVector(ActionEvent.MoveLeft.Name, ActionEvent.MoveRight.Name, ActionEvent.MoveForward.Name, ActionEvent.MoveBack.Name);

		Vector3 direction = new(inputVector.X, 0, inputVector.Y);

		Vector3 rotated = camera.Pose.AlignVector(direction);

		return rotated.Normalized();
	}
}
