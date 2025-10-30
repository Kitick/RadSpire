using Godot;
using System;

public partial class TopDownCameraPivot : Node3D {
    [Export] private Vector3 zoomPresets = new Vector3(3, 5, 7);
    [Export] private Vector3 rotationPresets = new Vector3(-30, -45, -60);
    [Export] private int curTiltIndex;
    [Export] private float zoomSpeed = 2.0f;
    private Camera3D camera;
    public override void _Ready() {
        base._Ready();
        curTiltIndex = 1;
        Position = calcYZPosVec();
        RotationDegrees = getRotVec();
        camera = GetChild<Camera3D>(0);
    }

    public override void _Process(double delta) {
        base._Process(delta);
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
