using Godot;
using System;

public partial class Player : CharacterBody3D {
	[Export] private float _defaultSpeed = 2.0f;
	[Export] private float _defaultSprintMultiplier = 2.0f;
	[Export] private float _defaultCrouchMultiplier = 0.5f;
	[Export] private float _defaultRotationSpeed = 4.0f;
	[Export] private float _defaultJumpVelocity = 4.5f;
	[Export] private float _defaultFallAcceleration = 9.8f;
	public override void _Ready() {

	}

	public override void _PhysicsProcess(double delta) {
		// Check for ESC to return to main menu
		if (Input.IsActionJustPressed("ui_cancel")) {
			GetTree().ChangeSceneToFile("res://Main Menu/Main_Menu.tscn");
			return;
		}

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
		if(Input.IsActionPressed("move_forward")) {
			direction.Z -= 1.0f;
		}
		if(Input.IsActionPressed("move_back")) {
			direction.Z += 1.0f;
		}
		direction = direction.Normalized();
		velocity.X = direction.X * finalSpeed;
		velocity.Z = direction.Z * finalSpeed;

		if(Input.IsActionPressed("jump") && IsOnFloor()) {
			velocity.Y = _defaultJumpVelocity;
		}
		if(!IsOnFloor()) {
			velocity.Y -= _defaultFallAcceleration * (float)delta;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
