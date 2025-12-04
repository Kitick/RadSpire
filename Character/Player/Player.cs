using System;
using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class Player : CharacterBody3D, ISaveable<PlayerData> {
	// Configuration
	[Export] private int InitalHealth = 100;
	[Export] private float SprintMultiplier = 3.0f;
	[Export] private float CrouchMultiplier = 0.5f;

	// Inventories
	public readonly Inventory Inventory = new Inventory(3, 5);
	public readonly Inventory Hotbar = new Inventory(1, 5);

	// Components
	public readonly Health Health;
	public readonly Movement Movement;
	public readonly Item3DIconPickup PickupComponent;

	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling }

	private readonly FiniteStateMachine<State> StateMachine = new(State.Idle);
	public State CurrentState => StateMachine.State;

	public event Action<State, State>? OnStateChange {
		add => StateMachine.OnStateChanged += value;
		remove => StateMachine.OnStateChanged -= value;
	}

	public Player() {
		Movement = new Movement(this);
		Health = new Health(InitalHealth);
		PickupComponent = new Item3DIconPickup();
	}

	public override void _Ready() {
		AddChild(PickupComponent);

		this.AddScene(Scenes.HUD);
	}

	public void Update(float dt, KeyInput keyInput) {
		if(Health.IsDead()) {
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		float multiplier = GetMultiplier();

		if(IsOnFloor()) {
			Movement.Move(keyInput.HorizontalInput, multiplier);

			if(keyInput.JumpPressed) { Movement.Jump(); }
		}

		Movement.Update(dt);

		UpdateMovementState(keyInput);
	}

	private void UpdateMovementState(KeyInput keyInput) {
		if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
		else if(!keyInput.IsMoving) { StateMachine.TransitionTo(State.Idle); }
		else if(keyInput.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
		else if(keyInput.CrouchHeld) { StateMachine.TransitionTo(State.Crouching); }
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
		Movement = Movement.Serialize(),
		Inventory = Inventory.Serialize()
	};

	public void Deserialize(in PlayerData data) {
		Health.Deserialize(data.Health);
		Movement.Deserialize(data.Movement);
		Inventory.Deserialize(data.Inventory);
	}
}

namespace SaveSystem {
	public readonly record struct PlayerData : ISaveData {
		public HealthData Health { get; init; }
		public MovementData Movement { get; init; }
		public InventoryData Inventory { get; init; }
	}
}
