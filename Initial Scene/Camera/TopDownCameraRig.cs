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
        float xOutPercent = 0.0f;
        float zOutPercent = 0.0f;
        bool inCenterZone = true;
        if(Mathf.Abs(positionDiff.X) > centerZone.X / 2) {
            inCenterZone = false;
            if(positionDiff.X > 0) {
                xOutPercent = Mathf.Abs(positionDiff.X - centerZone.X / 2) / (centerZone.X / 2);
            }
            else {
                xOutPercent = Mathf.Abs(positionDiff.X * -1 - centerZone.X / 2) / (centerZone.X / 2);
            }        
        }
        if(Mathf.Abs(positionDiff.Z) > centerZone.Y / 2) {
            inCenterZone = false;
            if(positionDiff.Z > 0) {
                zOutPercent = Mathf.Abs(positionDiff.Z - centerZone.Y / 2) / (centerZone.Y / 2);
            }
            else {
                zOutPercent = Mathf.Abs(positionDiff.Z * -1 - centerZone.Y / 2) / (centerZone.Y / 2);
            }
        }
        if(!inCenterZone) {
            float strength = Mathf.Clamp(Math.Max(xOutPercent, zOutPercent),  0.0f, 1.0f);
            float weight = 1f - Mathf.Exp(-followSpeed * delta * strength);
            GlobalPosition = GlobalPosition.Lerp(target.GlobalPosition, weight);
        }
    }

}
