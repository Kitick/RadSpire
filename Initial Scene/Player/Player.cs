using System;
using Godot;
using SaveSystem;

public partial class Player : CharacterBody3D, ISaveable<PlayerData> {
	[Export] private float DefaultSpeed = 2.0f;
	[Export] private float DefaultSprintMultiplier = 2.0f;
	[Export] private float DefaultCrouchMultiplier = 0.5f;
	[Export] private float DefaultRotationSpeed = 4.0f;
	[Export] private float DefaultJumpVelocity = 4.5f;
	[Export] private float DefaultFallAcceleration = 9.8f;

	public override void _Ready() {
		GameManager.Player = this;

		var hudScene = GD.Load<PackedScene>("res://HUD/UI.tscn");
		var hud = hudScene.Instantiate<CanvasLayer>(); // root of UI.tscn is CanvasLayer
		AddChild(hud); // adds HUD under Player
	}

	private static Vector3 GetHorizontalInput() {
		Vector3 direction = Vector3.Zero;

		if(Input.IsActionPressed("move_forward")) {
			direction.Z -= 1.0f;
		}
		if(Input.IsActionPressed("move_back")) {
			direction.Z += 1.0f;
		}
		if(Input.IsActionPressed("move_right")) {
			direction.X += 1.0f;
		}
		if(Input.IsActionPressed("move_left")) {
			direction.X -= 1.0f;
		}

		return direction.Normalized();
	}

	public override void _PhysicsProcess(double delta) {
		// Check for ESC to return to main menu
		if(Input.IsActionJustPressed("ui_cancel")) {
			GameManager.Save("autosave");
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

	// ISaveable implementation
	public PlayerData Serialize() {
		return new PlayerData {
			Position = GlobalPosition,
			Rotation = GlobalRotation,
			Velocity = Velocity,
			Health = 100f,
		};
	}

	public void Deserialize(in PlayerData data) {
		GlobalPosition = data.Position;
		GlobalRotation = data.Rotation;
		Velocity = data.Velocity;
	}
}
