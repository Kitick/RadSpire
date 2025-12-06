using System;
using Camera;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class Player : CharacterBody3D, IDamageable, ISaveable<PlayerData> {
	private static readonly Logger Log = new(nameof(Enemy), enabled: true);
	// Configuration
	[Export] private int InitalHealth = 100;
	[Export] private float SprintMultiplier = 2.25f;
	[Export] private float CrouchMultiplier = 0.5f;

	// Inventories
	public readonly Inventory Inventory = new Inventory(3, 5);
	public readonly Inventory Hotbar = new Inventory(1, 5);
	public InventoryManager InventoryManager = null!;

	// Components
	public readonly Health Health;
	public readonly Movement Movement;
	public readonly Item3DIconPickup PickupComponent;

	private ProgressBar HealthBar = null!;
	private PlayerAnimator Animator = null!;

	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling }

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

		this.AddScene(Scenes.HUD);
		AddChild(InventoryManager);
		AddInventoriesToInventoryManager();

		AddToGroup("Player");

		HealthBar = GetNode<ProgressBar>("/root/GameManager/Player/HUD/HealthBar");
		Animator =  GetNode<PlayerAnimator>("/root/GameManager/Player/Knight");

		HealthBar.MaxValue = Health.MaxHealth;
		HealthBar.Value = Health.CurrentHealth;
	}

	private void HandleHealthChanged(int newValue) {
		HealthBar.Value = newValue;
	}

	public void AddInventoriesToInventoryManager() {
		InventoryUI inventoryUI = GetNode<HUD>("HUD").GetNode<InventoryUI>("Inventory");
		Inventory.Name = "Inventory";
		InventoryManager.RegisterInventory(Inventory, inventoryUI);
		Hotbar hotbarUI = GetNode<HUD>("HUD").GetNode<Hotbar>("Hotbar");
		Hotbar.Name = "Hotbar";
		InventoryManager.RegisterInventory(Hotbar, hotbarUI);
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
		if (keyInput.AttackPressed) {
			Animator.PlaySlash();
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

		HandleHealthChanged(Health.CurrentHealth);

		if(Health.IsDead()) {
			Animator.PlayDie();
		}
	}

	public void Die() {
		var hud = GetNodeOrNull<HUD>("HUD");
		if(hud != null) {
			hud.ShowRespawnMenu();
			Log.Info("Player died, showing respawn menu");
		}
		else {
			Log.Error("Player.Die: Could not find HUD to show respawn menu");
		}
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
