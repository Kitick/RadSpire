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

	private static Vector3 GetHorizontalInput() {
		Vector3 direction = Vector3.Zero;

		if(Input.IsActionPressed("move_forward")) {
			direction.X += 1.0f;
		}
		if(Input.IsActionPressed("move_back")) {
			direction.X -= 1.0f;
		}
		if(Input.IsActionPressed("move_right")) {
			direction.Z += 1.0f;
		}
		if(Input.IsActionPressed("move_left")) {
			direction.Z -= 1.0f;
		}

		return direction.Normalized();
	}

	public override void _PhysicsProcess(double delta) {
		// Check for ESC to return to main menu
		if(Input.IsActionJustPressed("ui_cancel")) {
			GetTree().ChangeSceneToFile("res://Main Menu/Main_Menu.tscn");
			return;
		}

		float dt = (float)delta;
		float multiplier = 1.0f;

		if(Input.IsActionPressed("sprint")) {
			multiplier *= DefaultSprintMultiplier;
		}
		if(Input.IsActionPressed("crouch")) {
			multiplier *= DefaultCrouchMultiplier;
		}

		Vector3 horizontalInput = GetHorizontalInput();
		Vector3 newVelocity = horizontalInput * DefaultSpeed * multiplier;

		float fallVelocity = Velocity.Y;

		if(Input.IsActionPressed("jump") && IsOnFloor()) {
			fallVelocity += DefaultJumpVelocity;
		}

		if(!IsOnFloor()) {
			fallVelocity -= DefaultFallAcceleration * dt;
		}

		newVelocity.Y = fallVelocity;
		Velocity = newVelocity;

		MoveAndSlide();
	}
}
