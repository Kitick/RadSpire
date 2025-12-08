using System;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class Player : CharacterBody3D, IDamageable, ISaveable<PlayerData> {
	private static readonly Logger Log = new(nameof(Enemy), enabled: true);

	[ExportCategory("Player Config")]
	[Export] private int InitalHealth = 100;
	[Export] private float SprintMultiplier = 2.25f;
	[Export] private float CrouchMultiplier = 0.5f;

	[ExportCategory("Player Nodes")]
	[Export] private PlayerAnimator Animator = null!;

	// Inventories
	public readonly Inventory Inventory = new Inventory(3, 5);
	public readonly Inventory Hotbar = new Inventory(1, 5);
	public InventoryManager InventoryManager = null!;

	// Components
	public readonly Health Health;
	public readonly Movement Movement;
	public readonly Item3DIconPickup PickupComponent;

	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dead }

	private readonly StateMachine<State> StateMachine = new(State.Idle);
	public State CurrentState => StateMachine.CurrentState;

	public Player() {
		Movement = new Movement(this);
		Health = new Health(InitalHealth);
		PickupComponent = new Item3DIconPickup();
		InventoryManager = new InventoryManager();
	}

	public override void _Ready() {
		AddChild(PickupComponent);
		AddChild(InventoryManager);
	}

	public void Update(float dt, KeyInput keyInput) {
		if(Health.IsDead()) {
			StateMachine.TransitionTo(State.Dead);
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
		if(keyInput.AttackPressed) {
			StateMachine.TransitionTo(State.Attacking);
		}

		if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
		else if(!keyInput.IsMoving) { StateMachine.TransitionTo(State.Idle); }
		else if(keyInput.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
		else if(keyInput.CrouchHeld) { StateMachine.TransitionTo(State.Crouching); }
		else { StateMachine.TransitionTo(State.Walking); }
	}

	public void TakeDamage(int amount) {
		Health.CurrentHealth -= amount;
		Log.Info($"Player HP: {Health.CurrentHealth}");
	}

	private float GetMultiplier() {
		float multiplier = 1.0f;

		if(StateMachine.CurrentState == State.Sprinting) { multiplier *= SprintMultiplier; }
		if(StateMachine.CurrentState == State.Crouching) { multiplier *= CrouchMultiplier; }

		return multiplier;
	}

	public PlayerData Serialize() => new PlayerData {
		Health = Health.Serialize(),
		Movement = Movement.Serialize(),
		Inventory = Inventory.Serialize(),
		Hotbar = Hotbar.Serialize(),
	};

	public void Deserialize(in PlayerData data) {
		Health.Deserialize(data.Health);
		Movement.Deserialize(data.Movement);
		Inventory.Deserialize(data.Inventory);
		Hotbar.Deserialize(data.Hotbar);
	}
}

namespace SaveSystem {
	public readonly record struct PlayerData : ISaveData {
		public HealthData Health { get; init; }
		public MovementData Movement { get; init; }
		public InventoryData Inventory { get; init; }
		public InventoryData Hotbar { get; init; }
	}
}
