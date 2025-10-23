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

        if(Input.IsActionPressed("sprint")) {
            
        }
        if(Input.IsActionPressed("crouch")) {
            
        }
        if(Input.IsActionPressed("move_right")) {

        }
        if(Input.IsActionPressed("move_left")) {

        }
        if(Input.IsActionPressed("move_foward")) {

        }
        if(Input.IsActionPressed("move_back")) {

        }
        if(Input.IsActionPressed("jump")) {
            
        }
    }
}
