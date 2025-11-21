using System;
using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public partial class Player : CharacterBody3D, ISaveable<PlayerData> {
	[Export] private int InitalHealth = 100;
	[Export] private float SprintMultiplier = 2.0f;
	[Export] private float CrouchMultiplier = 0.5f;

	// Components
	private KeyInput KeyInput = null!;
	private Health Health = null!;
	private Movement Movement = null!;

	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling }

	private readonly FiniteStateMachine<State> StateMachine = new(State.Idle);
	public State CurrentState => StateMachine.State;

	public event Action<State, State>? OnStateChange {
		add => StateMachine.OnStateChanged += value;
		remove => StateMachine.OnStateChanged -= value;
	}

	public override void _Ready() {
		KeyInput = new KeyInput();
		Movement = new Movement(this);
		Health = new Health(InitalHealth);

		this.AddScene(Scenes.HUD);
	}

	public override void _PhysicsProcess(double delta) {
		if(Health.IsDead()) {
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		float dt = (float) delta;

		KeyInput.Update();

		float multiplier = GetMultiplier();

		Movement.Move(KeyInput.HorizontalInput, multiplier);

		if(KeyInput.JumpPressed && IsOnFloor()) {
			Movement.Jump();
		}

		Movement.Update(dt);

		UpdateMovementState();
	}

	public void AddCamera(CameraRig camera) {
		KeyInput.Camera = camera;
		camera.Target = this;
	}

	private void UpdateMovementState() {
		if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
		else if(!KeyInput.IsMoving) { StateMachine.TransitionTo(State.Idle); }
		else if(KeyInput.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
		else if(KeyInput.CrouchHeld) { StateMachine.TransitionTo(State.Crouching); }
		else { StateMachine.TransitionTo(State.Walking); }
	}

	private float GetMultiplier() {
		float multiplier = 1.0f;

		if(StateMachine.State == State.Sprinting) { multiplier *= SprintMultiplier; }
		if(StateMachine.State == State.Crouching) { multiplier *= CrouchMultiplier; }

		return multiplier;
	}

	public PlayerData Serialize() => new PlayerData {
		Health = Health.Serialize(),
		Movement = Movement.Serialize()
	};

	public void Deserialize(in PlayerData data) {
		Health.Deserialize(data.Health);
		Movement.Deserialize(data.Movement);
	}
}

namespace SaveSystem {
	public readonly record struct PlayerData : ISaveData {
		public HealthData Health { get; init; }
		public MovementData Movement { get; init; }
	}
}