using Godot;
using System;

public partial class Player : CharacterBody3D {
	[Export] private float DefaultSpeed = 2.0f;
	[Export] private float DefaultSprintMultiplier = 2.0f;
	[Export] private float DefaultCrouchMultiplier = 0.5f;
	[Export] private float DefaultRotationSpeed = 4.0f;
	[Export] private float DefaultJumpVelocity = 4.5f;
	[Export] private float DefaultFallAcceleration = 9.8f;

	public override void _Ready() {

	}

	public override void _PhysicsProcess(double delta) {
		// Check for ESC to return to main menu
		if(Input.IsActionJustPressed("ui_cancel")) {
			GetTree().ChangeSceneToFile("res://Main Menu/Main_Menu.tscn");
			return;
		}

		Vector3 velocity = Velocity;
		Vector3 direction = Vector3.Zero;
		float finalSpeed = DefaultSpeed;

		if(Input.IsActionPressed("sprint")) {
			finalSpeed *= DefaultSprintMultiplier;
		}
		if(Input.IsActionPressed("crouch")) {
			finalSpeed *= DefaultCrouchMultiplier;
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
			velocity.Y = DefaultJumpVelocity;
		}
		if(!IsOnFloor()) {
			velocity.Y -= DefaultFallAcceleration * (float)delta;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
