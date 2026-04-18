namespace Camera;

using System;
using Character;
using Godot;
using Services;
using Settings;

public sealed partial class CameraRig {
	[Export] public float PanSensitivity = 0.1f;
	[Export] public float FollowSpeed = 5.0f;
	[Export] public float ZoomSpeed = 1.0f;

	private float MouseRotateSensitivity => MouseKeyboardSettings.MouseSensitivity.Target;
	private float JoystickRotateSensitivity => ControllerSettings.ControllerSensitivity.Target;

	private bool IsPanning = false;
	private bool IsRotating = false;

	private Action? Unsubscribe;

	void InitInput() {
		Unsubscribe =
			ActionEvent.CameraPan.WhenPressed(() => HandlePan(true))
			+ ActionEvent.CameraPan.WhenReleased(() => HandlePan(false))
			+ ActionEvent.CameraRotate.WhenPressed(() => HandleRotate(true))
			+ ActionEvent.CameraRotate.WhenReleased(() => HandleRotate(false))
			+ ActionEvent.ZoomIn.WhenPressed(() => TryZoom(-ZoomSpeed))
			+ ActionEvent.ZoomOut.WhenPressed(() => TryZoom(ZoomSpeed))
			+ ActionEvent.CameraReset.WhenPressed(Reset)
			+ (() => InputSystem.Instance.OnMouseMoved -= HandleMouseMotion);

		InputSystem.Instance.OnMouseMoved += HandleMouseMotion;
	}

	private void HandlePan(bool pressed) {
		if(!IsPanning && pressed) {
			IsPanning = true;
			Drag.Start(Pose.Ground);
		}
		else if(IsPanning && !pressed) {
			IsPanning = false;
			Drag.End();
		}
	}

	private void HandleRotate(bool pressed) {
		if(!IsRotating && pressed) {
			IsRotating = true;
		}
		else if(IsRotating && !pressed) {
			IsRotating = false;
		}
	}

	private void HandleMouseMotion(InputEventMouseMotion motion) {
		if(IsPanning) {
			Vector2 relative = -motion.ScreenRelative * PanSensitivity;
			Vector2 delta = Pose.AlignVector(relative);
			Drag.Move(new Vector3(delta.X, 0, delta.Y));
		}
		if(IsRotating) {
			Vector2 delta = motion.ScreenRelative * MouseRotateSensitivity;
			Pose.Heading -= delta.X;
			Pose.Pitch += delta.Y;
		}
	}

	public void HandleJoystickRotation(float dt) {
		var joypads = Input.GetConnectedJoypads();
		if(joypads.Count == 0) { return; }

		int device = joypads[0];
		float x = Input.GetJoyAxis(device, JoyAxis.RightX);
		float y = Input.GetJoyAxis(device, JoyAxis.RightY);

		if(Mathf.Abs(x) > 0.1f) { Pose.Heading -= x * JoystickRotateSensitivity * dt; }
		if(Mathf.Abs(y) > 0.1f) { Pose.Pitch += y * JoystickRotateSensitivity * dt; }
	}

	private void TryZoom(float distanceDelta) {
		if(Target is Player player && player.ObjectPlacementManager?.IsPlacementActive == true) {
			return;
		}
		Pose.Distance += distanceDelta;
	}
}
