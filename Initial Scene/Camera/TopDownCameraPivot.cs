using Godot;
using System;

public partial class TopDownCameraPivot : Node3D {
    [Export] private Vector3 default_position = new Vector3(0, 8, 8);
    [Export] private Vector3 default_rotation = new Vector3(-45, 0, 0);
    private Camera3D camera;
    public override void _Ready() {
        base._Ready();
        Position = default_position;
        RotationDegrees = default_rotation;
        camera = GetChild<Camera3D>(0);
    }

	public override void _Process(double delta) {
		base._Process(delta);
	}
}
