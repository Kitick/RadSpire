using Godot;
using System;

public partial class Player : CharacterBody3D {
    [Export] private float _defaultSpeed = 5.0f;
    [Export] private float _defaultSprintMultiplier = 2.0f;
    [Export] private float _defaultCrouchMultiplier = 0.5f;
    [Export] private float _defaultRotationSpeed = 10.0f;
    [Export] private float _defaultJumpVelocity = 4.5f;
    [Export] private float _defaultFallAcceleration = 9.8f;
    public override void _Ready() {

    }
    
    public override void _PhysicsProcess(double delta) {
        Vector3 velocity = Velocity;
        Vector3 direction = Vector3.Zero;
        float finalSpeed = _defaultSpeed;

        if(Input.IsActionPressed("sprint")) {
            finalSpeed *= _defaultSprintMultiplier;
        }
        if(Input.IsActionPressed("crouch")) {
            finalSpeed *= _defaultCrouchMultiplier;
        }
        if(Input.IsActionPressed("move_right")) {
            direction.X += 1.0f;
        }
        if(Input.IsActionPressed("move_left")) {
            direction.X -= 1.0f;
        }   
        if(Input.IsActionPressed("move_foward")) {
            direction.Z -= 1.0f;
        }
        if(Input.IsActionPressed("move_back")) {
            direction.Z += 1.0f;
        }
        direction = direction.Normalized();
        velocity += direction * finalSpeed;

        if(Input.IsActionPressed("jump")) {
            velocity.Y = _defaultJumpVelocity;
        }
        if(!IsOnFloor()) {
            velocity.Y -= _defaultFallAcceleration * (float)delta;
        }
        Velocity = velocity;
        MoveAndSlide();
    }
}
