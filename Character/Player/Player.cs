using System;
using Godot;
using SaveSystem;

public partial class Player : Character, ISaveable<PlayerData> {
	[Export] private float DefaultSprintMultiplier = 2.0f;
	[Export] private float DefaultCrouchMultiplier = 0.5f;
	[Export] private float DefaultFriction = 10.0f;
	[Export] private float PlayerMaxHealth = 200f;
	[Signal] public delegate void SprintStartEventHandler();
	[Signal] public delegate void SprintEndEventHandler();
	[Signal] public delegate void CrouchStartEventHandler();
	[Signal] public delegate void CrouchEndEventHandler();
	private bool IsCrouching = false;
	private bool IsSprinting = false;

	public override void _Ready() {
		base._Ready();
		GameManager.Player = this;

		var hudScene = GD.Load<PackedScene>("res://HUD/UI.tscn");
		var hud = hudScene.Instantiate<CanvasLayer>(); // root of UI.tscn is CanvasLayer
		AddChild(hud); // adds HUD under Player
		CharacterName = "Player";
		MaxHealth = PlayerMaxHealth;
		Type = "Player";
	}

	public override void _PhysicsProcess(double delta) {
		MoveDirection = GetHorizontalInput();
		FaceDirection = MoveDirection;
		SpeedModifier = playerSpeed();
		base._PhysicsProcess(delta);
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

	private float playerSpeed() {
		float multiplier = 1.0f;

		if(Input.IsActionPressed("sprint")) {
			multiplier *= DefaultSprintMultiplier;
			if(!IsSprinting) {
				IsSprinting = true;
				EmitSignal(SignalName.SprintStart);
			}
		}
		else if(Input.IsActionPressed("crouch")) {
			multiplier *= DefaultCrouchMultiplier;
			if(!IsCrouching) {
				IsCrouching = true;
				EmitSignal(SignalName.CrouchStart);
			}
		}
		else {
			if(IsSprinting) {
				IsSprinting = false;
				EmitSignal(SignalName.SprintEnd);
			}
			if(IsCrouching) {
				IsCrouching = false;
				EmitSignal(SignalName.CrouchEnd);
			}
		}
		return multiplier;
	}

	// ISaveable implementation
	public PlayerData Serialize() {
		return new PlayerData {
			Character = base.Serialize(),
			DefaultSprintMultiplier = DefaultSprintMultiplier,
			DefaultCrouchMultiplier = DefaultCrouchMultiplier,
			DefaultFriction = DefaultFriction,
			PlayerMaxHealth = PlayerMaxHealth,
			IsCrouching = IsCrouching,
			IsSprinting = IsSprinting
		};
	}

	public void Deserialize(in PlayerData data) {
		base.Deserialize(data.Character);
		DefaultSprintMultiplier = data.DefaultSprintMultiplier;
		DefaultCrouchMultiplier = data.DefaultCrouchMultiplier;
		DefaultFriction = data.DefaultFriction;
		PlayerMaxHealth = data.PlayerMaxHealth;
		IsCrouching = data.IsCrouching;
		IsSprinting = data.IsSprinting;
	}
}
