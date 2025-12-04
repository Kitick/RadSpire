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
	public InventoryManager InventoryManager = null!;

	// Components
	public readonly KeyInput KeyInput;
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
		KeyInput = new KeyInput();
		Movement = new Movement(this);
		Health = new Health(InitalHealth);
		PickupComponent = new Item3DIconPickup();
		InventoryManager = new InventoryManager();
	}

	public override void _Ready() {
		AddChild(PickupComponent);
		AddChild(InventoryManager);

		this.AddScene(Scenes.HUD);
		AddInventoriesToInventoryManager();
	}

	public void AddInventoriesToInventoryManager() {
		InventoryUI inventoryUI = GetNode<HUD>("HUD").GetNode<InventoryUI>("Inventory");
		InventoryManager.RegisterInventory(Inventory, inventoryUI);
		Hotbar hotbarUI = GetNode<HUD>("HUD").GetNode<Hotbar>("Hotbar");
		InventoryManager.RegisterInventory(Hotbar, hotbarUI);
	}

	public override void _PhysicsProcess(double delta) {
		if(Health.IsDead()) {
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		float dt = (float) delta;

		KeyInput.Update();

		float multiplier = GetMultiplier();

		if(IsOnFloor()) {
			Movement.Move(KeyInput.HorizontalInput, multiplier);

			if(KeyInput.JumpPressed) {
				Movement.Jump();
			}
		}

		Movement.Update(dt);

		UpdateMovementState();
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
