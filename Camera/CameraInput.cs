using System;
using Core;
using Godot;

namespace Camera {
	public partial class CameraRig {
		public MouseButton PanMouseButton = MouseButton.Right;
		public MouseButton RotateMouseButton = MouseButton.Middle;

		public float MouseSensitivity = 0.01f;

		public override void _Input(InputEvent input) {
			if(input.IsActionPressed(Actions.CameraReset)) {
				Reset();
			}
			else if(input is InputEventMouseButton mouseEvent) {
				if(mouseEvent.ButtonIndex == PanMouseButton) { HandleMousePan(mouseEvent); }
				else if(mouseEvent.ButtonIndex == RotateMouseButton) { HandleMouseRotate(mouseEvent); }
			}
			else if(input is InputEventMouseMotion mouseMotion) {
				HandleMouseMotion(mouseMotion);
			}
		}

		private void HandleMousePan(InputEventMouseButton mouse) {
			if(State != CameraState.Panning && mouse.Pressed) {
				State = CameraState.Panning;
				Drag.Start(Pose.Ground);
			}
			else if(State == CameraState.Panning && !mouse.Pressed) {
				State = CameraState.Idle;
				Drag.End();
			}
		}

		private void HandleMouseRotate(InputEventMouseButton mouse) {

		}

		private void HandleMouseZoom(InputEventMouseMotion motion) {

		}

		private void HandleMouseMotion(InputEventMouseMotion motion) {
			if(State != CameraState.Panning) { return; }

			Vector2 delta2 = -motion.ScreenRelative * MouseSensitivity;
			Vector3 delta = new Vector3(delta2.X, 0, delta2.Y);

			Drag.Move(delta);
		}
	}
}
