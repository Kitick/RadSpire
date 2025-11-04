using System;
using Godot;
using SaveSystem;

public partial class TopDownCameraPivot : Node3D, ISaveable<CameraPivotData> {
	[Export] private Vector3 zoomPresets = new Vector3(5, 7, 9);
	[Export] private Vector3 rotationPresets = new Vector3(-30, -45, -60);
    [Export] private int curTiltIndex;
	[Export] private float zoomSpeed = 2.0f;
	[Export] private float zoomCoolDown = 1.0f;
	private Timer zoomTimer = new Timer();
	private Camera3D? camera;
	private float deltaTime;
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
		deltaTime = (float)delta;
		updateCameraPositionAndRotation();
	}

	public override void _Input(InputEvent @event) {
		if(zoomTimer.IsStopped()) {
			zoomTimer.Start(zoomCoolDown);
			handleMouseScroll(@event);
			handleTrackPad(@event);
		}
	}

	private void handleMouseScroll(InputEvent @event) {
		if(@event is InputEventMouseButton mouseScrollEvent && mouseScrollEvent.Pressed) {
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

	private void handleTrackPad(InputEvent @event) {
		if(@event is InputEventPanGesture panGestureEvent) {
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

    private void updateCameraPositionAndRotation(){
		Vector3 newPosition = calcYZPosVec();
		float weight = 1f - Mathf.Exp(-zoomSpeed * deltaTime);
		Position = Position.Lerp(newPosition, weight);
		Vector3 newRotationVec = getRotVec();
		Transform3D newRotation = Transform;
		newRotation.Basis = new Basis(Vector3.Right, Mathf.DegToRad(newRotationVec.X));
		Quaternion newRotationQ = new Quaternion(newRotation.Basis);
		Transform3D curRotation = Transform;
		Quaternion curRotationQ = new Quaternion(curRotation.Basis);
		curRotationQ = curRotationQ.Slerp(newRotationQ, weight);
		curRotation.Basis = new Basis(curRotationQ);
		Transform = curRotation;
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

	// ISaveable implementation
	public CameraPivotData Serialize() {
		return new CameraPivotData {
			TiltIndex = curTiltIndex,
			Position = Position,
			Rotation = RotationDegrees
		};
	}

	public void Deserialize(in CameraPivotData data) {
		curTiltIndex = data.TiltIndex;
		Position = data.Position;
		RotationDegrees = data.Rotation;
	}
}
