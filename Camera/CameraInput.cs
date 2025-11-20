using System;
using Core;
using Godot;

namespace Camera {
	public partial class CameraRig {
		public MouseButton PanMouseButton = MouseButton.Right;
		public MouseButton RotateMouseButton = MouseButton.Middle;

		public float PanSensitivity = 0.1f;
		public float RotateSensitivity = 0.5f;
		public float ZoomSpeed = 1.0f;

		private bool IsPanning = false;
		private bool IsRotating = false;

		public override void _Input(InputEvent input) {
			if(input.IsActionPressed(Actions.CameraReset)) {
				Reset();
			}
			else if(input is InputEventMouseButton mouseEvent) {
				if(mouseEvent.ButtonIndex == PanMouseButton) { HandleMouseButton(mouseEvent); }
				else if(mouseEvent.ButtonIndex == RotateMouseButton) { HandleMouseRotate(mouseEvent); }
				else if(mouseEvent.ButtonIndex == MouseButton.WheelUp || mouseEvent.ButtonIndex == MouseButton.WheelDown) {
					HandleMouseZoom(mouseEvent);
				}
			}
			else if(input is InputEventMouseMotion mouseMotion) {
				HandleMouseMotion(mouseMotion);
			}
		}

		private void HandleMouseButton(InputEventMouseButton mouse) {
			if(!IsPanning && mouse.Pressed) {
				IsPanning = true;
				Drag.Start(Pose.Ground);
			}
			else if(IsPanning && !mouse.Pressed) {
				IsPanning = false;
				Drag.End();
			}
		}

		private void HandleMouseRotate(InputEventMouseButton mouse) {
			if(!IsRotating && mouse.Pressed) {
				IsRotating = true;
			}
			else if(IsRotating && !mouse.Pressed) {
				IsRotating = false;
			}
		}

		private void HandleMouseZoom(InputEventMouseButton motion) {
			if(motion.ButtonIndex == MouseButton.WheelUp) {
				Pose.Distance -= ZoomSpeed;
			}
			else if(motion.ButtonIndex == MouseButton.WheelDown) {
				Pose.Distance += ZoomSpeed;
			}
		}

		private void HandleMouseMotion(InputEventMouseMotion motion) {
			if(IsPanning) {
				Vector2 delta2 = -motion.ScreenRelative * PanSensitivity;
				Vector3 delta = new Vector3(delta2.X, 0, delta2.Y);

				Drag.Move(delta);
			}
			else if(IsRotating) {
				Vector2 delta = motion.ScreenRelative * RotateSensitivity;

				Pose.Heading -= delta.X;
				Pose.Pitch += delta.Y;
			}
		}
	}
}
