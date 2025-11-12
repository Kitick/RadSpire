using System;
using System.Data.Common;
using Godot;
using SaveSystem;

public partial class Player : Character, ISaveable<PlayerData> {
	[Export] private float defaultSprintMultiplier = 2.0f;
	[Export] private float defaultCrouchMultiplier = 0.5f;
	[Export] private float defaultFriction = 10.0f;
	[Export] private float playerMaxHealth = 200f;
	[Signal] public delegate void PlayerMovementEventHandler(string action);
	[Signal] public delegate void SprintStartEventHandler();
	[Signal] public delegate void SprintEndEventHandler();
	[Signal] public delegate void CrouchStartEventHandler();
	[Signal] public delegate void CrouchEndEventHandler();
	private Vector3 horizontalInput = Vector3.Zero;
	private bool isCrouching = false;
	private bool isSprinting = false;

	public override void _Ready() {
		base._Ready();
		GameManager.Player = this;

		var hudScene = GD.Load<PackedScene>("res://HUD/UI.tscn");
		var hud = hudScene.Instantiate<CanvasLayer>(); // root of UI.tscn is CanvasLayer
		AddChild(hud); // adds HUD under Player
		setCharacterName("Player");
		setMaxHealth(playerMaxHealth);
		setType("Player");
	}

	public override void _PhysicsProcess(double delta) {
		horizontalInput = GetHorizontalInput();
		float multiplier = playerSpeed();
		setSpeedModifier(multiplier);

		base._PhysicsProcess(delta);

		Vector3 newVelocity;
		if (horizontalInput == Vector3.Zero) {
			newVelocity = Velocity;
			float weight = 1f - Mathf.Exp(-defaultFriction * (float)delta);
			newVelocity.X = Mathf.Lerp(newVelocity.X, 0.0f, weight);
			newVelocity.Z = Mathf.Lerp(newVelocity.Z, 0.0f, weight);
		}

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
			multiplier *= defaultSprintMultiplier;
			if (!isSprinting) {
				isSprinting = true;
				EmitSignal(SignalName.CrouchStart);
			}
		}
		else if (Input.IsActionPressed("crouch")) {
			multiplier *= defaultCrouchMultiplier;
			if (!isCrouching) {
				isCrouching = true;
				EmitSignal(SignalName.CrouchStart);
			}
		}
		else {
			if (isSprinting) {
				isSprinting = false;
				EmitSignal(SignalName.SprintEnd);
			}
			if (isCrouching) {
				isCrouching = false;
				EmitSignal(SignalName.CrouchEnd);
			}
		}
		return multiplier;
	}
	
	// ISaveable implementation
	public PlayerData Serialize() {
		return new PlayerData {
			DefaultSprintMultiplier = defaultSprintMultiplier,
			DefaultCrouchMultiplier = defaultCrouchMultiplier,
			DefaultFriction = defaultFriction,
			PlayerMaxHealth = playerMaxHealth,
			HorizontalInput = horizontalInput,
			IsCrouching = isCrouching,
			IsSprinting = isSprinting
		};
	}

	public void Deserialize(in PlayerData data) {
		defaultSprintMultiplier = data.DefaultSprintMultiplier;
		defaultCrouchMultiplier = data.DefaultCrouchMultiplier;
		defaultFriction = data.DefaultFriction;
		playerMaxHealth = data.PlayerMaxHealth;
		horizontalInput = data.HorizontalInput;
		isCrouching = data.IsCrouching;
		isSprinting = data.IsSprinting;
	}
}
