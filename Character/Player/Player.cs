using Components;
using Core;
using Godot;
using Services;
using ItemSystem;
using System;

namespace Character {
	public sealed partial class Player : CharacterBody3D, IHealth, IAttack, ISaveable<PlayerData> {
		private static readonly LogService Log = new(nameof(Enemy), enabled: true);

		[Export] private int InitialHealth = 100;
		[Export] private int InitialDamage = 10;

		[Export] private float SprintMultiplier = 2.25f;
		[Export] private float CrouchMultiplier = 0.5f;

		// Inventories
		public readonly Inventory Inventory = new Inventory(3, 5);
		public readonly Inventory Hotbar = new Inventory(1, 5);
		public InventoryManager InventoryManager = null!;

		// Components
		public Health Health { get; }
		public Attack Attack { get; }

		public readonly Movement Movement;
		public readonly Item3DIconPickup PickupComponent;

		// State Machine
		public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dead }

		private readonly StateMachine<State> StateMachine = new(State.Idle);

		public State CurrentState => StateMachine.CurrentState;
		public event Action<State, State>? OnStateChanged;

		public Player() {
			Movement = new Movement(this);
			PickupComponent = new Item3DIconPickup();
			InventoryManager = new InventoryManager();

			Health = new Health(InitialHealth);
			Attack = new Attack(InitialDamage);

			StateMachine.OnChange((from, to) => OnStateChanged?.Invoke(from, to));
		}

		public override void _Ready() {
			AddChild(PickupComponent);
			AddChild(InventoryManager);
		}

		public void Update(float dt, KeyInput keyInput) {
			if(this.IsDead()) {
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

		private float GetMultiplier() {
			float multiplier = 1.0f;

			if(StateMachine.CurrentState == State.Sprinting) { multiplier *= SprintMultiplier; }
			if(StateMachine.CurrentState == State.Crouching) { multiplier *= CrouchMultiplier; }

			return multiplier;
		}

		public PlayerData Export() => new PlayerData {
			Movement = Movement.Export(),
			Health = Health.Export(),
			Attack = Attack.Export(),
			Inventory = Inventory.Export(),
			Hotbar = Hotbar.Export(),
		};

		public void Import(PlayerData data) {
			Movement.Import(data.Movement);
			Health.Import(data.Health);
			Attack.Import(data.Attack);
			Inventory.Import(data.Inventory);
			Hotbar.Import(data.Hotbar);
		}
	}

	public readonly record struct PlayerData : ISaveData {
		public MovementData Movement { get; init; }
		public HealthData Health { get; init; }
		public AttackData Attack { get; init; }
		public InventoryData Inventory { get; init; }
		public InventoryData Hotbar { get; init; }
	}
}