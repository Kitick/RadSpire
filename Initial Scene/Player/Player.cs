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
	[Export] private float DefaultFriction = 10.0f;
	[Signal] public delegate void PlayerMovementEventHandler(string action);
	private Vector3 HorizontalInput = Vector3.Zero;
	private bool isCrouching = false;
	private bool isSprinting = false;
	private bool isMoving = false;
	private bool isInAir = false;

	public override void _Ready() {
		GameManager.Player = this;

		var hudScene = GD.Load<PackedScene>("res://HUD/UI.tscn");
		var hud = hudScene.Instantiate<CanvasLayer>(); // root of UI.tscn is CanvasLayer
		AddChild(hud); // adds HUD under Player
	}

	public override void _PhysicsProcess(double delta) {
		// Check for ESC to return to main menu
		if(Input.IsActionJustPressed("ui_cancel")) {
			GameManager.Save("autosave");
			GetTree().ChangeSceneToFile("res://Main Menu/Main_Menu.tscn");
			return;
		}

		float dt = (float)delta;

		float multiplier = playerSpeed();

		HorizontalInput = GetHorizontalInput();
		Vector3 newVelocity;
		if (HorizontalInput != Vector3.Zero) {
			newVelocity = HorizontalInput * DefaultSpeed * multiplier;

			matchRotationToDirection(HorizontalInput, multiplier, dt);
		}
		else {
			newVelocity = Velocity;
			float weight = 1f - Mathf.Exp(-DefaultFriction * dt);
			newVelocity.X = Mathf.Lerp(newVelocity.X, 0.0f, weight);
			newVelocity.Z = Mathf.Lerp(newVelocity.Z, 0.0f, weight);
		}
		
		if (newVelocity.Length() > 0.1f) {
			if (!isMoving) {
				isMoving = true;
				EmitSignal(SignalName.PlayerMovement, "move_start");
			}
		}
		else {
			if (isMoving) {
				isMoving = false;
				EmitSignal(SignalName.PlayerMovement, "move_stop");
			}
		}

		float fallVelocity = Velocity.Y;

		if (Input.IsActionPressed("jump") && IsOnFloor()) {
			fallVelocity += DefaultJumpVelocity;
			EmitSignal(SignalName.PlayerMovement, "jump");
			isInAir = true;
		}

		if (!IsOnFloor()) {
			fallVelocity -= DefaultFallAcceleration * dt;
		}
		else if(isInAir){
			EmitSignal(SignalName.PlayerMovement, "land");
			isInAir = false;
		}

		newVelocity.Y = fallVelocity;
		Velocity = newVelocity;

		MoveAndSlide();
	}

	private static Vector3 GetHorizontalInput() {
		Vector3 direction = Vector3.Zero;

		if (Input.IsActionPressed("move_forward")) {
			direction.Z -= 1.0f;
		}
		if (Input.IsActionPressed("move_back")) {
			direction.Z += 1.0f;
		}
		if (Input.IsActionPressed("move_right")) {
			direction.X += 1.0f;
		}
		if (Input.IsActionPressed("move_left")) {
			direction.X -= 1.0f;
		}

		return direction.Normalized();
	}
	
	private float playerSpeed() {
		float multiplier = 1.0f;

		if (Input.IsActionPressed("sprint")) {
			multiplier *= DefaultSprintMultiplier;
			if (!isSprinting) {
				isSprinting = true;
				EmitSignal(SignalName.PlayerMovement, "sprint_start");
				isMoving = true;
			}
		}
		else if (Input.IsActionPressed("crouch")) {
			multiplier *= DefaultCrouchMultiplier;
			if (!isCrouching) {
				isCrouching = true;
				EmitSignal(SignalName.PlayerMovement, "crouch_start");
				isMoving = true;
			}
		}
		else {
			if (isSprinting) {
				isSprinting = false;
				EmitSignal(SignalName.PlayerMovement, "sprint_stop");
			}
			if (isCrouching) {
				isCrouching = false;
				EmitSignal(SignalName.PlayerMovement, "crouch_stop");
			}
		}
		return multiplier;
	}

	private void matchRotationToDirection(Vector3 direction, float magnitude, float dt) {
		if(direction.Length() > 0.0f) {
			Vector3 newRotationVec = Vector3.Zero;
			newRotationVec.Y = Mathf.RadToDeg(Mathf.Atan2(direction.X, direction.Z));
			Transform3D newRotation = Transform;
			newRotation.Basis = new Basis(Vector3.Up, Mathf.DegToRad(newRotationVec.Y));
			Quaternion newRotationQ = new Quaternion(newRotation.Basis);
			Transform3D curRotation = Transform;
			Quaternion curRotationQ = new Quaternion(curRotation.Basis);
			float rotationSpeed = DefaultRotationSpeed * magnitude;
			float weight = 1f - Mathf.Exp(-rotationSpeed * dt);
			curRotationQ = curRotationQ.Slerp(newRotationQ, weight);
			curRotation.Basis = new Basis(curRotationQ);
			Transform = curRotation;
		}
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
