namespace Character {
	using Components;
	using Core;
	using Godot;
	using Services;
	using ItemSystem;
	using System;
	using Objects;

	public sealed partial class Player : CharacterBase, ISaveable<PlayerData> {
		private static readonly LogService Log = new(nameof(Player), enabled: true);

		[Export] private int InitialHealthValue = 100;
		[Export] private int InitialDamagePhysical = 10;
		[Export] private int InitialDamageMagic = 0;
		[Export] private int InitialDefensePhysical = 5;
		[Export] private int InitialDefenseMagic = 2;

		protected override int InitialHealth => InitialHealthValue;
		protected override (int phys, int mag) InitialDamage => (InitialDamagePhysical, InitialDamageMagic);
		protected override (int phys, int mag) InitialDefense => (InitialDefensePhysical, InitialDefenseMagic);

		[Export] private float SprintMultiplier = 2.25f;
		[Export] private float CrouchMultiplier = 0.5f;

		// Inventories
		public readonly Inventory Inventory = new Inventory(3, 5);
		public readonly Inventory Hotbar = new Inventory(1, 5);
		public readonly InventoryManager InventoryManager = new InventoryManager();

		// Components
		public readonly Movement Movement;
		public readonly Item3DIconPickup PickupComponent = new Item3DIconPickup();
		public readonly UseItem UseItemComponent = new UseItem();
		public ObjectPickup? ObjectPickup { get; private set; }
		private ObjectPickupUI? ObjectPickupUI;
		private Action? UnsubscribeInteract;
		private Action? UnsubscribeInteract2;

		public Player() {
			Movement = new Movement(this);
			Inventory.Name = "Inventory";
			Hotbar.Name = "Hotbar";
		}

		public override void _Ready() {
			base._Ready();
			PickupComponent.HandleInteractInput = false;
			AddChild(PickupComponent);
			AddChild(InventoryManager);
			AddChild(UseItemComponent);
			SetupChildren();
			this.Hurt(50); // For testing purposes, start the player hurt.
		}

		public override void _ExitTree() {
			base._ExitTree();
			UnsubscribeInteract?.Invoke();
			UnsubscribeInteract2?.Invoke();
			ObjectPickupUI?.Dispose();
			ObjectPickup = null;
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

		private void SetupChildren() {
			UseItemComponent.User = this;

			InteractionArea? interactionArea = GetNodeOrNull<InteractionArea>("InteractionArea");
			if(interactionArea == null) {
				Log.Error("Player InteractionArea not found. ObjectPickup not initialized.");
				return;
			}
			ObjectPickup = new ObjectPickup(interactionArea, InventoryManager);
			ObjectPickupUI = new ObjectPickupUI(ObjectPickup);
			UnsubscribeInteract = ActionEvent.Interact.WhenPressed(() => {
				if(PickupComponent.HasItemsInRange) {
					PickupComponent.PickupItem();
					return;
				}
				ObjectPickup.AttemptPickup();
			});

			UnsubscribeInteract2 = ActionEvent.Interact2.WhenPressed(() => {
				if(ObjectPickup.currentTargetObjectNode == null) {
					return;
				}
				ObjectPickup.currentTargetObjectNode.Interact(this);
			});
		}

		public void ConfigureObjectPickup(WorldObjectManager worldObjectManager) {
			if(ObjectPickup == null) {
				Log.Error("ConfigureObjectPickup called before ObjectPickup was initialized.");
				return;
			}
			ObjectPickup.WorldObjectManager = worldObjectManager;
		}

		public PlayerData Export() => new PlayerData {
			Movement = Movement.Export(),
			Health = Health.Export(),
			Offense = Offense.Export(),
			Defense = Defense.Export(),
			Inventory = Inventory.Export(),
			Hotbar = Hotbar.Export(),
		};

		public void Import(PlayerData data) {
			Movement.Import(data.Movement);
			Health.Import(data.Health);
			Offense.Import(data.Offense);
			Defense.Import(data.Defense);
			Inventory.Import(data.Inventory);
			Hotbar.Import(data.Hotbar);
		}
	}

	public readonly record struct PlayerData : ISaveData {
		public MovementData Movement { get; init; }
		public HealthData Health { get; init; }
		public OffenseData Offense { get; init; }
		public DefenseData Defense { get; init; }
		public InventoryData Inventory { get; init; }
		public InventoryData Hotbar { get; init; }
	}
}