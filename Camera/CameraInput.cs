using System;
using Core;
using Godot;

namespace Camera {
	public sealed partial class CameraRig {
		public float PanSensitivity = 0.1f;
		public float RotateSensitivity = 0.5f;
		public float ZoomSpeed = 1.0f;

		private bool IsPanning = false;
		private bool IsRotating = false;

		public override void _Input(InputEvent input) {
			if(input.IsActionPressed(Actions.CameraReset)) {
				Reset();
			}

			if(input is InputEventMouseMotion mouseMotion) {
				HandleMouseMotion(mouseMotion);
			}
			else {
				HandlePan(input.IsActionPressed(Actions.CameraPan));
				HandleRotate(input.IsActionPressed(Actions.CameraRotate));
				HandleZoom(input);
			}
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

		private void HandleZoom(InputEvent input) {
			if(input.IsActionPressed(Actions.ZoomIn)) {
				Pose.Distance -= ZoomSpeed;
			}
			else if(input.IsActionPressed(Actions.ZoomOut)) {
				Pose.Distance += ZoomSpeed;
			}
		}

		private void HandleMouseMotion(InputEventMouseMotion motion) {
			if(IsPanning) {
				Vector2 relative = -motion.ScreenRelative * PanSensitivity;

				Vector2 delta = relative.Rotated(-Pose.RadHDG);

				Drag.Move(new Vector3(delta.X, 0, delta.Y));
			}
			if(IsRotating) {
				Vector2 delta = motion.ScreenRelative * RotateSensitivity;

				Pose.Heading -= delta.X;
				Pose.Pitch += delta.Y;
			}
		}
	}
}
