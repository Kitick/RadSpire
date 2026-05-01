namespace Character;

using System;
using System.Collections.Generic;
using Character.Recruitment;
using Components;
using GameWorld;
using Godot;
using InventorySystem;
using InventorySystem.Interface;
using ItemSystem;
using ItemSystem.Icons;
using ItemSystem.WorldObjects;
using Root;
using Services;

public sealed partial class Player : CharacterBase, ISaveable<PlayerData>, IAttackModifier, IDamageBlocker, IRadiation {
	private static readonly LogService Log = new(nameof(Player), enabled: true);

	[Export] public MeshInstance3D SwordMesh = null!;
	[Export] public MeshInstance3D ShieldMesh = null!;
	[Export] public MeshInstance3D StaffMesh = null!;
	[Export] public MeshInstance3D HelmetMesh = null!;
	[Export] public Node3D StaffCastPoint = null!;
	[Export] public PackedScene RadiationBoltScene = null!;
	[Export] public StringName StaffAttackAnimation = "";

	[Export] private int InitialHealthValue = 100;
	[Export] public int InitialDamageValue = 3;
	[Export] private int InitialDefenseValue = 5;
	[Export] private int IronHeadpieceDefenseBonus = 2;

	protected override int InitialHealth => InitialHealthValue;
	protected override int InitialDamage => InitialDamageValue;
	protected override int InitialDefense => InitialDefenseValue;

	[Export] private float SprintMultiplier = 4.0f;
	[Export] private float CrouchMultiplier = 0.5f;
	[Export] private float DodgeSpeedMultiplier = 3.0f;
	[Export] private float ComboResetTime = 0.75f;
	[Export] private float ComboHit2Multiplier = 1.15f;
	[Export] private float ComboHit3Multiplier = 1.30f;
	[Export] private float StaffAttackCooldown = 0.45f;
	[Export] private int StaffProjectileDamage = 10;
	[Export] private float StaffRadiationPerShot = 0.01f;
	[Export] private float DamageFlashTime = 0.12f;
	[Export] private float BossHitKnockbackForce = 28.0f;
	[Export] private float BossHitKnockbackDecay = 8.0f;
	[Export] private int ShieldMaxBlockedHits = 3;
	[Export] private float BlockMoveMultiplier = 0.45f;

	// Inventories
	public readonly Inventory Inventory = new(3, 5);
	public readonly Inventory Hotbar = new(1, 5);
	public readonly InventoryManager InventoryManager = new();

	// Components
	public readonly Movement Movement;
	public readonly Item3DIconPickup PickupComponent = new();
	public readonly UseItem UseItemComponent = new();
	public readonly EquipItem EquipItemComponent = new();
	public ObjectPickup? ObjectPickup { get; private set; }
	public ObjectPlacementManager? ObjectPlacementManager { get; private set; }
	public bool IsBuildModeActive => BuildModeController?.IsBuildModeActive == true;
	public bool IsDraggingBuildFurniture => BuildModeController?.IsDraggingFurniture == true;
	private ObjectPlacementUI? ObjectPlacementUI;
	private ObjectPickupUI? ObjectPickupUI;
	private ObjectHoverTargetingController? ObjectHoverTargetingController;
	private ObjectHoverOutlineUI? ObjectHoverOutlineUI;
	private BuildModeController? BuildModeController;
	private event Action? OnExit;

	public Radiation Radiation { get; private set; } = new Radiation(secondsToFatalDose: 30 * 60);
	public int BaseMaxHealth { get; private set; }
	public bool IsSleeping = false;
	public Vector3 LocationBeforeSleep = Vector3.Zero;
	public Vector3 RotationBeforeSleep = Vector3.Zero;

	private float SleepHealAccumulator = 0f;
	private float SleepHealthPerSecond = 2f;
	private float SleepRadiationPerSecond = 5f / (30f * 60f);

	public bool HoldingSword = false;
	public bool HoldingStaff = false;
	public WeaponBase? EquippedWeapon { get; set; }
	private Vector3 DodgeDirection = Vector3.Zero;
	private Animator? Animator;
	private int ComboIndex = 0;
	private float ComboTimer = 0f;
	private bool BufferedAttack = false;
	private float CurrentAttackMultiplier = 1.0f;
	private float StaffAttackCooldownTimer = 0f;
	private bool IsStaffAttackActive = false;
	private float DamageFlashTimer = 0f;
	private Vector3 HitKnockbackVelocity = Vector3.Zero;
	private bool IsBlockHeld = false;
	private int RemainingShieldBlocks = 0;
	private readonly List<StandardMaterial3D> FlashMaterials = [];
	private static readonly StringName ComboAttack1 = new("1H_Melee_Attack_Slice_Diagonal");
	private static readonly StringName ComboAttack2 = new("1H_Melee_Attack_Stab");
	private static readonly StringName ComboAttack3 = new("1H_Melee_Attack_Chop");
	public NPCRecruitmentManager? NPCRecruitmentManager { get; set; }

	public Player() {
		Movement = new Movement(this);
		Inventory.Name = "Inventory";
		Hotbar.Name = "Hotbar";
	}

	public override void _Ready() {
		base._Ready();
		BaseMaxHealth = InitialHealth;
		PickupComponent.HandleInteractInput = false;
		AddToGroup(Group.Player.ToString());
		Animator = GetNodeOrNull<Animator>("Model/AnimationPlayer");
		Animator?.SetAttackSpeed(3.0f);
		InitializeDamageFlash();
		RefreshCombatStats();
		AddToGroup(Group.Player.ToString());
		AddChild(PickupComponent);
		AddChild(InventoryManager);
		AddChild(UseItemComponent);
		AddChild(EquipItemComponent);
		SetupChildren();
		Inventory.OnInventoryChanged += RefreshOwnedEquipmentEffects;
		Hotbar.OnInventoryChanged += RefreshOwnedEquipmentEffects;
		RefreshOwnedEquipmentEffects();
	}

	private void SubscribeInputActions() {
		OnExit += ActionEvent.Interact.WhenPressed(() => {
			if(PickupComponent.HasItemsInRange) {
				PickupComponent.PickupItem();
				return;
			}
			if(BuildModeController != null && BuildModeController.IsBuildModeActive) {
				return;
			}
			if(ObjectPickup!.CurrentTargetObjectNode == null) {
				return;
			}
			ObjectPickup.CurrentTargetObjectNode.Interact(this);
		});

		OnExit += ActionEvent.Enter.WhenPressed(() => {
			if(ObjectPickup?.CurrentTargetObjectNode == null) {
				return;
			}
			ObjectPickup.CurrentTargetObjectNode.Interact(this);
		});

		OnExit += ActionEvent.AssignNPC.WhenPressed(() => {
			if(BuildModeController != null && BuildModeController.IsBuildModeActive) {
				return;
			}
			if(ObjectPickup?.CurrentTargetObjectNode?.Data == null) {
				return;
			}
			NPCRecruitmentManager?.TryAssignFollowingNpc(ObjectPickup.CurrentTargetObjectNode.Data);
		});

		OnExit += ActionEvent.Pickup.WhenPressed(ObjectPickup!.AttemptPickup);

		OnExit += ActionEvent.Place.WhenPressed(() => {
			if(ObjectPlacementManager == null) {
				Log.Error("Place action pressed but ObjectPlacementManager is not initialized.");
				return;
			}
			ObjectPlacementManager.PlaceRequested();
		});

		OnExit += ActionEvent.PlaceCancel.WhenPressed(() => {
			if(ObjectPlacementManager == null) {
				Log.Error("PlaceCancel action pressed but ObjectPlacementManager is not initialized.");
				return;
			}
			ObjectPlacementManager.PlaceCanceled();
		});

		OnExit += ActionEvent.BuildMode.WhenPressed(() => {
			BuildModeController?.ToggleBuildMode();
		});
	}

	public override void _ExitTree() {
		base._ExitTree();
		OnExit?.Invoke();
		ObjectPickupUI?.Dispose();
		ObjectHoverOutlineUI?.Dispose();
		ObjectPickup = null;
		ObjectPlacementUI = null;
		ObjectPlacementManager = null;
		ObjectHoverTargetingController = null;
		ObjectHoverOutlineUI = null;
		BuildModeController = null;
	}

	public void Update(float dt, KeyInput keyInput) {
		IsBlockHeld = keyInput.BlockHeld;
		UpdateDamageFlash(dt);

		if(IsSleeping) {
			Radiation.Deccumulate(dt, SleepRadiationPerSecond);
			Health.Max = Math.Max(1, (int) Math.Round(BaseMaxHealth * (1f - Radiation.Level)));
			SleepHealAccumulator += SleepHealthPerSecond * dt;
			int healAmount = (int) SleepHealAccumulator;
			if(healAmount > 0) {
				this.Heal(healAmount);
				SleepHealAccumulator -= healAmount;
			}
			Velocity = Vector3.Zero;
			if(this.IsDead()) {
				StateMachine.TransitionTo(State.Dead);
				return;
			}
			return;
		}
		Radiation.Accumulate(dt);
		Health.Max = Math.Max(1, (int) Math.Round(BaseMaxHealth * (1f - Radiation.Level)));
		if(StaffAttackCooldownTimer > 0f) {
			StaffAttackCooldownTimer = Math.Max(0f, StaffAttackCooldownTimer - dt);
		}
		if(ComboTimer > 0f) {
			ComboTimer -= dt;
			if(ComboTimer <= 0f && CurrentState != State.Attacking) {
				ResetCombo();
			}
		}

		if(this.IsDead()) {
			StateMachine.TransitionTo(State.Dead);
			return;
		}

		if(CurrentState == State.Dodging) {
			Movement.Move(DodgeDirection, DodgeSpeedMultiplier);
			Movement.Update(dt);
			return;
		}

		if(HitKnockbackVelocity.LengthSquared() > 0.001f) {
			GlobalPosition += HitKnockbackVelocity * dt;
			HitKnockbackVelocity = HitKnockbackVelocity.Lerp(Vector3.Zero, BossHitKnockbackDecay * dt);
		}

		if(CurrentState == State.Hit) {
			return;
		}

		if(CurrentState == State.Blocking) {
			if(IsOnFloor()) {
				Movement.Move(keyInput.HorizontalInput, BlockMoveMultiplier);
			}
			UpdateMovementState(keyInput);
			Movement.Update(dt);
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
		if(CurrentState == State.Blocking) {
			if(!keyInput.BlockHeld || !HasUsableShield()) {
				StopBlocking(keyInput);
			}
			else {
				return;
			}
		}

		if(keyInput.BlockPressed) {
			StartBlocking();
			if(CurrentState == State.Blocking) {
				return;
			}
		}

		if(keyInput.DodgePressed) {
			StartDodge(keyInput);
			return;
		}

		if(keyInput.AttackPressed && (BuildModeController == null || !BuildModeController.IsBuildModeActive)) {
			if(HoldingStaff) {
				TryStartStaffAttack();
				return;
			}

			if(StateMachine.CurrentState == State.Attacking) {
				QueueComboAttack();
			}
			else {
				StartComboAttack();
			}
			return;
		}

		if(StateMachine.CurrentState == State.Attacking) { return; }

		if(!IsOnFloor()) {
			StateMachine.TransitionTo(State.Falling);
		}
		else if(!keyInput.IsMoving) {
			StateMachine.TransitionTo(State.Idle);
		}
		else if(keyInput.SprintHeld) {
			StateMachine.TransitionTo(State.Sprinting);
		}
		else if(keyInput.CrouchHeld) {
			StateMachine.TransitionTo(State.Crouching);
		}
		else {
			StateMachine.TransitionTo(State.Walking);
		}
	}

	public override void OnAttackFinished() {
		if(IsStaffAttackActive) {
			IsStaffAttackActive = false;
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		if(BufferedAttack && ComboIndex < 2) {
			BufferedAttack = false;
			ComboIndex++;
			StartComboAttack();
			return;
		}

		BufferedAttack = false;
		if(ComboIndex < 2) {
			ComboIndex++;
			ComboTimer = ComboResetTime;
		}
		else {
			ResetCombo();
		}

		StateMachine.TransitionTo(State.Idle);
	}

	public float GetAttackMultiplier() => CurrentAttackMultiplier;

	private float GetMultiplier() {
		float multiplier = 1.0f;

		if(StateMachine.CurrentState == State.Sprinting) { multiplier *= SprintMultiplier; }
		if(StateMachine.CurrentState == State.Crouching) { multiplier *= CrouchMultiplier; }

		return multiplier;
	}

	private void StartDodge(KeyInput keyInput) {
		if(CurrentState == State.Dodging) { return; }

		Vector3 dir = keyInput.HorizontalInput;
		if(dir.Length() < Numbers.EPSILON) {
			dir = -GlobalTransform.Basis.Z;
			dir.Y = 0f;
		}

		DodgeDirection = dir.Normalized();
		// Skip idle recovery when sprinting to keep it snappy.
		Animator?.SetDodgeIdleRecovery(!keyInput.SprintHeld);
		SetDodgeAnimationFromInput();
		StateMachine.TransitionTo(State.Dodging);
	}

	private void StartBlocking() {
		if(!HasUsableShield()) {
			return;
		}
		if(CurrentState == State.Attacking || CurrentState == State.Dodging || CurrentState == State.Hit || CurrentState == State.Dead) {
			return;
		}

		ResetCombo();
		BufferedAttack = false;
		IsStaffAttackActive = false;
		Velocity = Vector3.Zero;
		StateMachine.TransitionTo(State.Blocking);
	}

	private void StopBlocking(KeyInput keyInput) {
		if(CurrentState != State.Blocking) {
			return;
		}

		if(!IsOnFloor()) {
			StateMachine.TransitionTo(State.Falling);
		}
		else if(!keyInput.IsMoving) {
			StateMachine.TransitionTo(State.Idle);
		}
		else if(keyInput.SprintHeld) {
			StateMachine.TransitionTo(State.Sprinting);
		}
		else if(keyInput.CrouchHeld) {
			StateMachine.TransitionTo(State.Crouching);
		}
		else {
			StateMachine.TransitionTo(State.Walking);
		}
	}

	private void StartComboAttack() {
		if(ComboTimer <= 0f) {
			ResetCombo();
		}

		CurrentAttackMultiplier = GetCurrentComboMultiplier();
		BufferedAttack = false;
		ComboTimer = ComboResetTime;
		if(Animator != null) {
			Animator.SetAttackAnimation(GetCurrentComboAnimation());
			if(StateMachine.CurrentState == State.Attacking) {
				Animator.PlayAttackNow();
				return;
			}
		}
		StateMachine.TransitionTo(State.Attacking);
	}

	private void QueueComboAttack() {
		if(ComboIndex >= 2) { return; }
		BufferedAttack = true;
		ComboTimer = ComboResetTime;
	}

	private void TryStartStaffAttack() {
		if(Animator == null || RadiationBoltScene == null || StaffCastPoint == null) {
			return;
		}
		if(CurrentState == State.Attacking || StaffAttackCooldownTimer > 0f) {
			return;
		}

		ResetCombo();
		BufferedAttack = false;
		CurrentAttackMultiplier = 1.0f;
		StaffAttackCooldownTimer = StaffAttackCooldown;
		IsStaffAttackActive = true;

		if(!StaffAttackAnimation.Equals(default(StringName))) {
			Animator.SetAttackAnimation(StaffAttackAnimation);
		}

		SpawnStaffProjectile();
		StateMachine.TransitionTo(State.Attacking);
	}

	private void ResetCombo() {
		ComboIndex = 0;
		ComboTimer = 0f;
		BufferedAttack = false;
		CurrentAttackMultiplier = 1.0f;
	}

	private StringName GetCurrentComboAnimation() => ComboIndex switch {
		0 => ComboAttack1,
		1 => ComboAttack2,
		_ => ComboAttack3,
	};

	private float GetCurrentComboMultiplier() => ComboIndex switch {
		0 => 1.0f,
		1 => ComboHit2Multiplier,
		_ => ComboHit3Multiplier,
	};

	public int GetMeleeDamage() {
		int damage = InitialDamageValue;
		if(EquippedWeapon?.VisualType == WeaponBase.WeaponVisualType.Sword) {
			damage += EquippedWeapon.BaseAttack;
		}
		return damage;
	}

	public void RefreshCombatStats() {
		if(Offense != null) {
			Offense.Damage = GetMeleeDamage();
		}

		if(Defense != null) {
			Defense.Armor = GetCurrentDefenseArmor();
		}
	}

	private int GetCurrentDefenseArmor() {
		int armor = InitialDefenseValue;
		if(HasOwnedItem(ItemID.HeadpieceIron)) {
			armor += IronHeadpieceDefenseBonus;
		}
		return armor;
	}

	private void RefreshOwnedEquipmentEffects() {
		RefreshShieldState();
		RefreshCombatStats();
		UpdateEquipmentVisuals();
	}

	private void RefreshShieldState() {
		if(HasOwnedShield()) {
			if(RemainingShieldBlocks <= 0) {
				RemainingShieldBlocks = ShieldMaxBlockedHits;
			}
			else {
				RemainingShieldBlocks = Math.Min(RemainingShieldBlocks, ShieldMaxBlockedHits);
			}
			return;
		}

		RemainingShieldBlocks = 0;
		IsBlockHeld = false;
		if(CurrentState == State.Blocking) {
			StateMachine.TransitionTo(State.Idle);
		}
	}

	private int GetStaffProjectileDamage() {
		int baseDamage = EquippedWeapon?.VisualType == WeaponBase.WeaponVisualType.Staff
			? EquippedWeapon.BaseAttack
			: StaffProjectileDamage;
		return Offense.RollDamage(baseDamage);
	}

	private void SpawnStaffProjectile() {
		if(RadiationBoltScene.Instantiate() is not RadiationBolt bolt) {
			return;
		}

		Radiation.Level += StaffRadiationPerShot;
		GetTree().CurrentScene?.AddChild(bolt);
		bolt.GlobalTransform = StaffCastPoint.GlobalTransform;
		Vector3 direction = -StaffCastPoint.GlobalTransform.Basis.Z;
		bolt.Init(this, direction, GetStaffProjectileDamage());
	}

	public void Sleep(Vector3 Location, Vector3 Rotation, float healthPerSecond, float radiationPerSecond) {
		if(IsSleeping) {
			return;
		}

		SleepHealthPerSecond = healthPerSecond;
		SleepRadiationPerSecond = radiationPerSecond;
		IsSleeping = true;
		LocationBeforeSleep = GlobalTransform.Origin;
		RotationBeforeSleep = GlobalRotation;
		GlobalPosition = Location;
		GlobalRotation = Rotation;
		Velocity = Vector3.Zero;
		Animator?.Play(new StringName("Lie_Idle"));
	}

	public void EndSleep() {
		if(!IsSleeping) {
			return;
		}

		IsSleeping = false;
		SleepHealAccumulator = 0f;
		GlobalPosition = LocationBeforeSleep;
		GlobalRotation = RotationBeforeSleep;
		Velocity = Vector3.Zero;
		Animator?.Play(new StringName("Idle"));
	}

	public override void OnDodgeFinished() => StateMachine.TransitionTo(State.Idle);

	public override void OnHitFinished() => StateMachine.TransitionTo(State.Idle);

	public override bool ShouldLoopBlockingAnimation() => CurrentState == State.Blocking && IsBlockHeld && HasUsableShield();

	public void TriggerBossHitReaction(Vector3 sourcePosition) {
		if(this.IsDead()) {
			return;
		}

		Vector3 direction = GlobalPosition - sourcePosition;
		direction.Y = 0f;
		if(direction.LengthSquared() < Numbers.EPSILON) {
			direction = -GlobalTransform.Basis.Z;
			direction.Y = 0f;
		}

		Vector3 faceTarget = sourcePosition;
		faceTarget.Y = GlobalPosition.Y;
		if((faceTarget - GlobalPosition).LengthSquared() > Numbers.EPSILON) {
			LookAt(faceTarget, Vector3.Up, true);
		}

		HitKnockbackVelocity = direction.Normalized() * BossHitKnockbackForce;
		if(CurrentState == State.Blocking && HasUsableShield()) {
			return;
		}

		Animator?.PlayHitNow();
		StateMachine.TransitionTo(State.Hit);
	}

	private void SetDodgeAnimationFromInput() {
		if(Animator == null) { return; }

		Vector2 input = Input.GetVector(ActionEvent.MoveLeft.Name, ActionEvent.MoveRight.Name, ActionEvent.MoveForward.Name, ActionEvent.MoveBack.Name);
		if(input.Length() < Numbers.EPSILON) {
			return;
		}

		float ax = Mathf.Abs(input.X);
		float az = Mathf.Abs(input.Y);

		if(ax > az) {
			if(input.X < 0f) { Animator.SetDodgeAnimation(new StringName("Dodge_Left")); } else { Animator.SetDodgeAnimation(new StringName("Dodge_Right")); }
		}
		else {
			Animator.SetDodgeAnimation(new StringName("Dodge_Forward"));
		}
	}

	private void InitializeDamageFlash() {
		FlashMaterials.Clear();

		Node modelRoot = GetNodeOrNull<Node>("Model/Rig/Skeleton3D") ?? GetNodeOrNull<Node>("Model");
		if(modelRoot == null) {
			return;
		}

		foreach(MeshInstance3D mesh in GetFlashableMeshes(modelRoot)) {

			if(mesh == SwordMesh || mesh == ShieldMesh || mesh == StaffMesh) {
				continue;
			}

			if(mesh.GetActiveMaterial(0) is not StandardMaterial3D baseMat) {
				continue;
			}

			StandardMaterial3D flashMat = (StandardMaterial3D) baseMat.Duplicate();
			mesh.SetSurfaceOverrideMaterial(0, flashMat);
			FlashMaterials.Add(flashMat);
		}

		Health.OnChanged += (from, to) => {
			if(to.Max < from.Max) { return; }
			if(to.Current < from.Current) {
				DamageFlashTimer = DamageFlashTime;
				SetDamageFlash(true);
			}
		};
	}

	private IEnumerable<MeshInstance3D> GetFlashableMeshes(Node node) {
		foreach(Node child in node.GetChildren()) {
			if(child is MeshInstance3D mesh) {
				yield return mesh;
			}

			foreach(MeshInstance3D nestedMesh in GetFlashableMeshes(child)) {
				yield return nestedMesh;
			}
		}
	}

	private void UpdateDamageFlash(float dt) {
		if(DamageFlashTimer <= 0f) {
			return;
		}

		DamageFlashTimer -= dt;
		if(DamageFlashTimer <= 0f) {
			SetDamageFlash(false);
		}
	}

	private void SetDamageFlash(bool enabled) {
		Color color = enabled ? new Color(1.25f, 0.55f, 0.55f) : Colors.White;
		foreach(StandardMaterial3D material in FlashMaterials) {
			material.AlbedoColor = color;
		}
	}

	private void UpdateEquipmentVisuals() {
		if(ShieldMesh != null) {
			ShieldMesh.Visible = HasOwnedShield();
		}

		if(StaffMesh != null) {
			StaffMesh.Visible = HoldingStaff;
		}

		if(HelmetMesh != null) {
			HelmetMesh.Visible = HasOwnedItem(ItemID.HeadpieceIron);
		}
	}

	private bool HasOwnedItem(StringName itemId) {
		foreach(ItemSlot slot in Inventory.ItemSlots) {
			if(!slot.IsEmpty() && slot.Item?.Id == itemId) {
				return true;
			}
		}

		foreach(ItemSlot slot in Hotbar.ItemSlots) {
			if(!slot.IsEmpty() && slot.Item?.Id == itemId) {
				return true;
			}
		}

		return false;
	}

	private bool HasOwnedShield() => HasOwnedItem(ItemID.ShieldWood);

	private bool HasUsableShield() => HasOwnedShield() && RemainingShieldBlocks > 0;

	private bool RemoveOwnedItem(StringName itemId) {
		if(RemoveOwnedItem(Hotbar, itemId)) {
			return true;
		}

		return RemoveOwnedItem(Inventory, itemId);
	}

	private static bool RemoveOwnedItem(Inventory inventory, StringName itemId) {
		for(int i = 0; i < inventory.ItemSlots.Length; i++) {
			ItemSlot slot = inventory.ItemSlots[i];
			if(slot.IsEmpty() || slot.Item?.Id != itemId) {
				continue;
			}

			return inventory.RemoveItem(inventory.GetRow(i), inventory.GetColumn(i), 1);
		}

		return false;
	}

	private void BreakShield() {
		IsBlockHeld = false;
		RemainingShieldBlocks = 0;
		_ = RemoveOwnedItem(ItemID.ShieldWood);
		Animator?.PlayHitNow();
		StateMachine.TransitionTo(State.Hit);
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
		ObjectHoverTargetingController = new ObjectHoverTargetingController();
		AddChild(ObjectHoverTargetingController);
		ObjectHoverTargetingController.Initialize(this, ObjectPickup);
		ObjectHoverOutlineUI = new ObjectHoverOutlineUI(ObjectHoverTargetingController);
		ObjectPlacementManager = new ObjectPlacementManager();
		AddChild(ObjectPlacementManager);
		ObjectPlacementUI = new ObjectPlacementUI();
		AddChild(ObjectPlacementUI);
		ObjectPlacementUI.Initialize(ObjectPlacementManager);

		SubscribeInputActions();
	}

	public void ConfigureObjectPickup(WorldObjectManager worldObjectManager) {
		if(ObjectPickup == null) {
			Log.Error("ConfigureObjectPickup called before ObjectPickup was initialized.");
			return;
		}
		ObjectPickup.WorldObjectManager = worldObjectManager;
	}

	public void ConfigureObjectPlacement(WorldObjectManager worldObjectManager, GameManager gameManager, Hotbar playerHotbar) {
		if(ObjectPlacementManager == null) {
			Log.Error("ConfigureObjectPlacement called before ObjectPlacementManager was initialized.");
			return;
		}
		ObjectPlacementManager.Initialize(worldObjectManager, InventoryManager, gameManager, playerHotbar, this);
		BuildModeController ??= new BuildModeController();
		if(BuildModeController.GetParent() == null) {
			AddChild(BuildModeController);
		}
		if(gameManager.HUDRef != null) {
			BuildModeController.Initialize(
				this,
				InventoryManager,
				ObjectPlacementManager,
				ObjectPlacementUI!,
				ObjectHoverTargetingController!,
				worldObjectManager,
				gameManager,
				gameManager.HUDRef.GetBuildUI()
			);
		}
	}

	public PlayerData Export() => new() {
		Movement = Movement.Export(),
		Health = Health.Export(),
		Offense = Offense.Export(),
		Defense = Defense.Export(),
		Radiation = Radiation.Export(),
		Inventory = Inventory.Export(),
		Hotbar = Hotbar.Export(),
		ShieldRemainingBlocks = RemainingShieldBlocks,
	};

	public void Import(PlayerData data) {
		Movement.Import(data.Movement);
		Health.Import(data.Health);
		Offense.Import(data.Offense);
		RefreshCombatStats();
		Defense.Import(data.Defense);
		Radiation.Import(data.Radiation);
		Inventory.Import(data.Inventory);
		Hotbar.Import(data.Hotbar);
		RemainingShieldBlocks = data.ShieldRemainingBlocks;
		RefreshOwnedEquipmentEffects();
	}

	public bool TryBlockDamage(int amount) {
		if(amount <= 0 || CurrentState != State.Blocking || !HasUsableShield()) {
			return false;
		}

		RemainingShieldBlocks = Math.Max(0, RemainingShieldBlocks - 1);
		if(RemainingShieldBlocks == 0) {
			BreakShield();
		}
		return true;
	}
}

public readonly record struct PlayerData : ISaveData {
	public MovementData Movement { get; init; }
	public HealthData Health { get; init; }
	public OffenseData Offense { get; init; }
	public DefenseData Defense { get; init; }
	public RadiationData Radiation { get; init; }
	public InventoryData Inventory { get; init; }
	public InventoryData Hotbar { get; init; }
	public int ShieldRemainingBlocks { get; init; }
}
