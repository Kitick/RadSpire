using Godot;
using System;
using System.Runtime;

public partial class TopDownCameraRig : Node3D {
    [Export] public Node3D target;
    [Export] private Vector2 centerZone = new Vector2(5, 4);
    [Export] private float followSpeed = 5.0f;

    private Vector3 targetPosition;

    public override void _Ready() {
        if(target == null) {
            GD.Print("TD Camera Rig needs a target");
        }
        else {
            GD.Print("TD Camera Rig target is set");
            targetPosition = target.GlobalPosition;
            GlobalPosition = targetPosition;
        }
    }

    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
        followTarget(dt);
    }
    
    private void followTarget(float delta) {
        Vector3 targetPosition = target.GlobalPosition;
        Vector3 curPosition = GlobalPosition;
        Vector3 positionDiff = targetPosition - curPosition;
        bool inCenterZone = true;
        if(Math.Abs(positionDiff.X) > centerZone.X / 2) {
            inCenterZone = false;
        }
        if(Math.Abs(positionDiff.Z) > centerZone.Y / 2) {
            inCenterZone = false;
        }
        if(!inCenterZone) {
            float weight = 1f - Mathf.Exp(-followSpeed * delta);
            GlobalPosition = GlobalPosition.Lerp(target.GlobalPosition, weight);
        }
    }

}
