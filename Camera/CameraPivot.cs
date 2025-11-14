using System;
using Core;
using Godot;
using SaveSystem;

public partial class CameraPivot : Node3D, ISaveable<CameraPivotData> {
	[Export] private Vector3 zoomPresets = new Vector3(5, 7, 9);
	[Export] private Vector3 rotationPresets = new Vector3(-30, -45, -60);
	[Export] private int curTiltIndex;
	[Export] private float zoomSpeed = 2.0f;
	[Export] private float zoomCoolDown = 1.0f;
	private Timer zoomTimer = new Timer();
	private Camera3D? camera;
	private float dt;
	[Signal] public delegate void ZoomChangedEventHandler(float zoomRatio, float titleAngle, float distance);

	public override void _EnterTree() {
		camera = GetNode<Camera3D>("Camera3D");
		camera.Current = true;
	}

	public override void _Ready() {
		curTiltIndex = 1;
		Position = calcYZPosVec();
		RotationDegrees = getRotVec();
		zoomTimer.OneShot = true;
		AddChild(zoomTimer);
		CallDeferred("emit_signal", SignalName.ZoomChanged, zoomPresets[curTiltIndex] / zoomPresets[1], rotationPresets[curTiltIndex], zoomPresets[curTiltIndex]);
	}

	public override void _PhysicsProcess(double delta) {
		dt = (float)delta;
		updateCameraPositionAndRotation();
	}

	public override void _Input(InputEvent input) {
		if(zoomTimer.IsStopped()) {
			zoomTimer.Start(zoomCoolDown);
			handleMouseScroll(input);
			handleTrackPad(input);
		}
	}

	private void updateCameraPositionAndRotation() {
		Vector3 newPosition = calcYZPosVec();
		Position = Position.SmoothLerp(newPosition, zoomSpeed, dt);
		Vector3 newRotationVec = getRotVec();

		this.ApplyRotation(Vector3.Right, Mathf.DegToRad(newRotationVec.X), zoomSpeed, dt);
	}

	private void handleMouseScroll(InputEvent input) {
		if(input is InputEventMouseButton mouseScrollEvent && mouseScrollEvent.Pressed) {
			switch(mouseScrollEvent.ButtonIndex) {
				case MouseButton.WheelUp:
					changeTiltIndex(false);
					break;
				case MouseButton.WheelDown:
					changeTiltIndex(true);
					break;
			}
		}
	}

	private void handleTrackPad(InputEvent input) {
		if(input is InputEventPanGesture panGestureEvent) {
			if(panGestureEvent.Delta.Y < 0) {
				changeTiltIndex(true);
			}
			else if(panGestureEvent.Delta.Y > 0) {
				changeTiltIndex(false);
			}
		}
	}

	private void changeTiltIndex(bool zoomOut) {
		if(zoomOut) {
			GD.Print("Zooming Out");
			if(curTiltIndex < 2) {
				curTiltIndex++;
				EmitSignal(SignalName.ZoomChanged, zoomPresets[curTiltIndex] / zoomPresets[1], rotationPresets[curTiltIndex], zoomPresets[curTiltIndex]);
			}
		}
		else {
			GD.Print("Zooming In");
			if(curTiltIndex > 0) {
				curTiltIndex--;
				EmitSignal(SignalName.ZoomChanged, zoomPresets[curTiltIndex] / zoomPresets[1], rotationPresets[curTiltIndex], zoomPresets[curTiltIndex]);
			}
		}
	}

	private Vector3 calcYZPosVec() {
		Vector3 curPosition = new Vector3(0, 0, 0);
		curPosition.Y = zoomPresets[curTiltIndex] * Mathf.Cos(Mathf.DegToRad(90 - -rotationPresets[curTiltIndex]));
		curPosition.Z = zoomPresets[curTiltIndex] * Mathf.Sin(Mathf.DegToRad(90 - -rotationPresets[curTiltIndex]));
		return curPosition;
	}

	private Vector3 getRotVec() {
		Vector3 curRotation = new Vector3(0, 0, 0);
		curRotation.X = rotationPresets[curTiltIndex];
		return curRotation;
	}

	public CameraPivotData Serialize() => new CameraPivotData {
		TiltIndex = curTiltIndex,
		Position = Position,
		Rotation = RotationDegrees,
	};

	public void Deserialize(in CameraPivotData data) {
		curTiltIndex = data.TiltIndex;
		Position = data.Position;
		RotationDegrees = data.Rotation;
	}
}

namespace SaveSystem {
	public readonly record struct CameraPivotData : ISaveData {
		public int TiltIndex { get; init; }
		public Vector3 Position { get; init; }
		public Vector3 Rotation { get; init; }
	}
}