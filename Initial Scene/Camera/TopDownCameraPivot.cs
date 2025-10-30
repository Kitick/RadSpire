using Godot;
using System;

public partial class TopDownCameraPivot : Node3D {
    [Export] private Vector3 zoomPresets = new Vector3(3, 5, 7);
    [Export] private Vector3 rotationPresets = new Vector3(-30, -45, -60);
    [Export] private int curTiltIndex;
    [Export] private float zoomSpeed = 2.0f;
    [Export] private float zoomCoolDown = 1.0f;
    private Timer zoomTimer = new Timer();
    private Camera3D camera;
    public override void _Ready() {
        base._Ready();
        curTiltIndex = 1;
        Position = calcYZPosVec();
        RotationDegrees = getRotVec();
        camera = GetChild<Camera3D>(0);
        zoomTimer.OneShot = true;
        AddChild(zoomTimer);
    }

    public override void _Process(double delta) {
        base._Process(delta);
    }

	public override void _Input(InputEvent @event) {
        base._Input(@event);
        if(@event is InputEventMouseButton mouseScrollEvent) {
            if(mouseScrollEvent.Pressed) {
                if(zoomTimer.IsStopped()) {
                    zoomTimer.Start(zoomCoolDown);
                    switch(mouseScrollEvent.ButtonIndex) {
                        case MouseButton.WheelUp:
                            if(curTiltIndex < 2) {
                                curTiltIndex++;
                            }
                            break;
                        case MouseButton.WheelDown:
                            if(curTiltIndex > 0) {
                                curTiltIndex--;
                            }
                            break;
                    }
                    Position = calcYZPosVec();
                    RotationDegrees = getRotVec();
                }

            }
        }
	}

    private Vector3 calcYZPosVec() {
        Vector3 curPosition = new Vector3(0, 0, 0);
        curPosition.Y = zoomPresets[curTiltIndex] * Mathf.Cos(90 - Mathf.DegToRad(rotationPresets[curTiltIndex]));
        curPosition.Z = zoomPresets[curTiltIndex] * Mathf.Sin(90 - Mathf.DegToRad(rotationPresets[curTiltIndex]));
        return curPosition;
    }

    private Vector3 getRotVec() {
        Vector3 curRotation = new Vector3(0, 0, 0);
        curRotation.X = rotationPresets[curTiltIndex];
        return curRotation;
    }
}
