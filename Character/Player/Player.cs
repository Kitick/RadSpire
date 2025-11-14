using System;
using Godot;
using SaveSystem;

public partial class Player : CharacterBody3D, ISaveable<PlayerData> {
	private const string HUD = "res://HUD/UI.tscn";

	private const string JUMP = "jump";
	private const string SPRINT = "sprint";
	private const string CROUCH = "crouch";

	private const string FORWARD = "move_forward";
	private const string BACK = "move_back";
	private const string LEFT = "move_left";
	private const string RIGHT = "move_right";

	private const float EPSILON = 0.01f;

	[Export] private float BaseSpeed = 2.0f;
	[Export] private float BaseRotationSpeed = 4.0f;

	[Export] private float SprintMultiplier = 2.0f;
	[Export] private float CrouchMultiplier = 0.5f;
	[Export] private float Friction = 10.0f;

	[Export] private Vector3 JumpVelocity = 4.5f * Vector3.Up;
	private Vector3 Acceleration => StateMachine.State == State.Falling ? 9.8f * Vector3.Down : Vector3.Zero;

	private Vector3 HorizontalInput = Vector3.Zero;

	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling }

	private FiniteStateMachine<State> StateMachine = new(State.Idle);
	public State CurrentState => StateMachine.State;

	public event Action<State, State>? OnStateChange {
		add => StateMachine.OnStateChanged += value;
		remove => StateMachine.OnStateChanged -= value;
	}

	public override void _Ready() {
		GameManager.Player = this;

		var hudScene = GD.Load<PackedScene>(HUD);
		var hud = hudScene.Instantiate<CanvasLayer>(); // root of UI.tscn is CanvasLayer
		AddChild(hud); // adds HUD under Player
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;

		HorizontalInput = GetHorizontalInput();

		float multiplier = GetPlayerSpeed();

		if(HorizontalInput != Vector3.Zero) {
			var move = HorizontalInput * BaseSpeed * multiplier;

			Velocity = new Vector3(move.X, Velocity.Y, move.Z);
		}
		else {
			float weight = 1f - Mathf.Exp(-Friction * dt);

			var x = Mathf.Lerp(Velocity.X, 0.0f, weight);
			var z = Mathf.Lerp(Velocity.Z, 0.0f, weight);

			Velocity = new Vector3(x, Velocity.Y, z);
		}

		if(Input.IsActionPressed(JUMP) && IsOnFloor()) {
			Velocity += JumpVelocity;
		}

		Velocity += Acceleration * dt;

		UpdateMovementState();
		MatchRotationToDirection(Velocity, multiplier, dt);
		MoveAndSlide();
	}

	private void UpdateMovementState() {
		if(!IsOnFloor()) {
			StateMachine.TransitionTo(State.Falling);
			return;
		}

		if(HorizontalInput.Length() < EPSILON) {
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		if(Input.IsActionPressed(SPRINT)) {
			StateMachine.TransitionTo(State.Sprinting);
			return;
		}

		if(Input.IsActionPressed(CROUCH)) {
			StateMachine.TransitionTo(State.Crouching);
			return;
		}

		StateMachine.TransitionTo(State.Walking);
	}

	private static Vector3 GetHorizontalInput() {
		Vector3 direction = Vector3.Zero;

		if(Input.IsActionPressed(FORWARD)) { direction += Vector3.Forward; }
		if(Input.IsActionPressed(BACK)) { direction += Vector3.Back; }
		if(Input.IsActionPressed(RIGHT)) { direction += Vector3.Right; }
		if(Input.IsActionPressed(LEFT)) { direction += Vector3.Left; }

		return direction.Normalized();
	}

	private float GetPlayerSpeed() {
		float multiplier = 1.0f;

		if(StateMachine.State == State.Sprinting) { multiplier *= SprintMultiplier; }
		if(StateMachine.State == State.Crouching) { multiplier *= CrouchMultiplier; }

		return multiplier;
	}

	private void MatchRotationToDirection(Vector3 direction, float magnitude, float dt) {
		if(direction.Length() < EPSILON) { return; }

		Vector3 newRotationVec = Vector3.Zero;
		newRotationVec.Y = Mathf.RadToDeg(Mathf.Atan2(direction.X, direction.Z));
		Transform3D newRotation = Transform;
		newRotation.Basis = new Basis(Vector3.Up, Mathf.DegToRad(newRotationVec.Y));
		Quaternion newRotationQ = new Quaternion(newRotation.Basis);
		Transform3D curRotation = Transform;
		Quaternion curRotationQ = new Quaternion(curRotation.Basis);
		float rotationSpeed = BaseRotationSpeed * magnitude;
		float weight = 1f - Mathf.Exp(-rotationSpeed * dt);
		curRotationQ = curRotationQ.Slerp(newRotationQ, weight);
		curRotation.Basis = new Basis(curRotationQ);
		Transform = curRotation;
	}

	// ISaveable implementation
	public PlayerData Serialize() {
		return new PlayerData {

		};
	}

	public void Deserialize(in PlayerData data) {

	}
}

namespace SaveSystem {
	public readonly record struct PlayerData : ISaveData {

	}
}