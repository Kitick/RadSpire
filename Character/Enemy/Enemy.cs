namespace Character;

using System;
using Components;
using Godot;
using ItemSystem;
using Root;
using Services;

public sealed partial class Enemy : CharacterBase, ISaveable<EnemyData> {
	private static readonly LogService Log = new(nameof(Enemy), enabled: true);
	public string Id { get; set; } = Guid.NewGuid().ToString();

	[Export] public EnemyType EnemyType { get; set; } = EnemyType.None;

	[Export] private int InitialHealthValue = 50;
	[Export] private int InitialDamageValue = 5;
	[Export] private int InitialDefenseValue = 0;

	[Export] private float KnockbackForce = 15f;
	[Export] private float KnockbackDecay = 12f;
	[Export] private float DamageFlashTime = 1.0f;
	[Export] private float StunDuration = 0.4f;
	[Export] private float AttackDistance = 1.5f;
	[Export] private float ChaseStopDistance = 1.5f;
	[Export] private float DetectionRadius = 15.0f;
	[Export] private float SprintDistance = 5.0f;
	[Export] private float HealthBarWidth = 1.4f;
	[Export] private float HealthBarHeight = 0.12f;
	[Export] private float HealthBarYOffset = 2.2f;
	[Export] private float DamageNumberYOffset = 1.6f;
	[Export] private Color HealthBarFillColor = new(0.2f, 0.9f, 0.2f);
	[Export] private Color HealthBarBackColor = new(0f, 0f, 0f, 0.6f);
	[Export] private float DamageNumberLifetime = 0.9f;
	[Export] private float DamageNumberRise = 0.6f;
	[Export] private float DamageNumberHorizontalJitter = 0.25f;
	[Export] private Color DamageNumberColor = new(1f, 0.85f, 0.2f);
	[Export] private int DamageNumberFontSize = 55;
	[Export] private PackedScene? HitSparkScene;
	[Export] private Node3D? StaffCastPoint;
	[Export] private PackedScene? RadiationBoltScene;
	[Export] private StringName StaffAttackAnimation = default;
	[Export] private float RangedAttackCooldown = 1.4f;
	[Export] private float RangedProjectileSpeed = 10.0f;

	protected override int InitialHealth => InitialHealthValue;
	protected override int InitialDamage => InitialDamageValue;
	protected override int InitialDefense => InitialDefenseValue;

	// Components
	private readonly Movement Movement;
	private readonly ChaseAI AI;
	private Node3D? AttackTarget;

	// Hit feedback
	private Vector3 KnockbackVelocity = Vector3.Zero;
	private float DamageFlashTimer = 0f;
	private float StunTimer = 0f;
	private MeshInstance3D? EnemyMesh;
	private StandardMaterial3D? FlashMaterial;
	private Node3D? HealthUIRoot;
	private Sprite3D? HealthBarBack;
	private Sprite3D? HealthBarFill;
	private Label3D? HealthLabel;
	private Animator? Animator;
	private float AttackCooldownTimer = 0f;

	public void SetTarget(Node3D target) {
		AI.SetTarget(target);
		AttackTarget = target;
	}

	public Enemy() {
		Movement = new Movement(this);
		AI = new ChaseAI(this);
	}

	public override void _Ready() {
		base._Ready();
		AddToGroup(Group.Enemy.ToString());

		EnemyMesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		Animator = GetNodeOrNull<Animator>("Model/AnimationPlayer");
		Animator?.SetAttackSpeed(1.5f);
		AI.AttackDistance = AttackDistance;
		AI.StopDistance = ChaseStopDistance;
		AI.DetectionRadius = DetectionRadius;
		AI.SprintDistance = SprintDistance;
		SetupHealthUI();

		if(EnemyMesh != null) {
			if(EnemyMesh.GetActiveMaterial(0) is StandardMaterial3D baseMat) {
				FlashMaterial = baseMat.Duplicate() as StandardMaterial3D;
				EnemyMesh.SetSurfaceOverrideMaterial(0, FlashMaterial);
			}
		}

		Health.OnChanged += (from, to) => {
			int damageTaken = from.Current - to.Current;
			if(damageTaken > 0) {
				SpawnDamageNumber(damageTaken);
				SpawnHitSpark();
			}

			UpdateHealthUI();
			if(to.Current >= from.Current) {
				return;
			}

			DamageFlashTimer = DamageFlashTime;
			StunTimer = Math.Max(StunTimer, StunDuration);
			SetDamageFlash(true);

			if(AttackTarget == null || !IsInstanceValid(AttackTarget)) {
				return;
			}

			Vector3 direction = GlobalPosition - AttackTarget.GlobalPosition;
			direction.Y = 0;

			if(direction.LengthSquared() < 0.0001f) {
				return;
			}

			KnockbackVelocity = direction.Normalized() * KnockbackForce;
		};

		UpdateHealthUI();
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float) delta;

		if(this.IsDead()) {
			StateMachine.TransitionTo(State.Dead);
			return;
		}

		if(DamageFlashTimer > 0f) {
			DamageFlashTimer -= dt;

			if(DamageFlashTimer <= 0f) {
				SetDamageFlash(false);
			}
		}

		if(AttackCooldownTimer > 0f) {
			AttackCooldownTimer = Math.Max(0f, AttackCooldownTimer - dt);
		}

		if(StunTimer > 0f) {
			StunTimer -= dt;
		}
		else {
			AI.Update();
			Movement.Move(AI.HorizontalInput, 1);
			Movement.Update(dt);
			if(EnemyType == EnemyType.RadiationCaster && AI.AttackPressed && AttackTarget != null && IsInstanceValid(AttackTarget)) {
				Movement.Face(AttackTarget.GlobalPosition - GlobalPosition, dt);
			}
		}

		if(KnockbackVelocity.LengthSquared() > 0.001f) {
			GlobalPosition += KnockbackVelocity * dt;
			KnockbackVelocity = KnockbackVelocity.Lerp(Vector3.Zero, KnockbackDecay * dt);
		}

		UpdateMovementState();
	}

	private void SetDamageFlash(bool enabled) {
		if(FlashMaterial == null) {
			return;
		}

		FlashMaterial.AlbedoColor = enabled ? new Color(2f, 0f, 0f) : Colors.White;
	}

	private void SetupHealthUI() {
		HealthUIRoot = GetNodeOrNull<Node3D>("HealthUI");
		HealthBarBack = GetNodeOrNull<Sprite3D>("HealthUI/HealthBarBack");
		HealthBarFill = GetNodeOrNull<Sprite3D>("HealthUI/HealthBarFill");
		HealthLabel = GetNodeOrNull<Label3D>("HealthUI/HealthLabel");

		if(HealthUIRoot == null) {
			HealthUIRoot = new Node3D { Name = "HealthUI" };
			AddChild(HealthUIRoot);
		}

		if(HealthBarBack == null) {
			HealthBarBack = new Sprite3D { Name = "HealthBarBack" };
			HealthUIRoot.AddChild(HealthBarBack);
		}

		if(HealthBarFill == null) {
			HealthBarFill = new Sprite3D { Name = "HealthBarFill" };
			HealthUIRoot.AddChild(HealthBarFill);
		}

		if(HealthLabel == null) {
			HealthLabel = new Label3D { Name = "HealthLabel" };
			HealthUIRoot.AddChild(HealthLabel);
		}

		HealthUIRoot.Position = new Vector3(0f, HealthBarYOffset, 0f);

		SetupSprite(HealthBarBack, HealthBarBackColor, HealthBarWidth, HealthBarHeight);
		SetupSprite(HealthBarFill, HealthBarFillColor, HealthBarWidth, HealthBarHeight);

		HealthLabel.FontSize = 18;
		HealthLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
		HealthLabel.Position = new Vector3(0f, HealthBarHeight * 1.8f, 0f);
	}

	private static void SetupSprite(Sprite3D sprite, Color color, float width, float height) {
		if(sprite.Texture == null) {
			Image image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
			image.Fill(Colors.White);
			sprite.Texture = ImageTexture.CreateFromImage(image);
		}

		sprite.Modulate = color;
		sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
		sprite.PixelSize = 1f;
		sprite.Scale = new Vector3(width, height, 1f);
		sprite.Centered = true;
	}

	private void UpdateHealthUI() {
		if(HealthBarFill == null || HealthLabel == null || HealthUIRoot == null) {
			return;
		}

		float pct = Mathf.Clamp(this.Percent(), 0f, 1f);

		Vector3 scale = HealthBarFill.Scale;
		scale.X = HealthBarWidth * pct;
		scale.Y = HealthBarHeight;
		scale.Z = 1f;
		HealthBarFill.Scale = scale;
		HealthBarFill.Position = new Vector3(-(HealthBarWidth - scale.X) * 0.5f, 0f, 0f);

		HealthLabel.Text = $"{Health.Current}/{Health.Max}";
		HealthUIRoot.Visible = Health.Current > 0 && Health.Current < Health.Max;
	}

	private void SpawnDamageNumber(int amount) {
		if(amount <= 0) { return; }

		Label3D label = new() {
			Text = $"-{amount}",
			FontSize = DamageNumberFontSize,
			Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
			Modulate = DamageNumberColor
		};

		float jitter = (float) GD.RandRange(-DamageNumberHorizontalJitter, DamageNumberHorizontalJitter);
		GetParent()?.AddChild(label);
		label.GlobalPosition = GlobalPosition + new Vector3(jitter, DamageNumberYOffset, 0f);

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(label, "global_position",
			label.GlobalPosition + new Vector3(0f, DamageNumberRise, 0f),
			DamageNumberLifetime).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		Color endColor = DamageNumberColor;
		endColor.A = 0f;
		tween.TweenProperty(label, "modulate", endColor, DamageNumberLifetime);
		tween.Finished += () => {
			if(IsInstanceValid(label)) {
				label.QueueFree();
			}
		};
	}

	private void SpawnHitSpark() {
		if(HitSparkScene?.Instantiate() is not Node3D spark) {
			return;
		}

		GetParent()?.AddChild(spark);
		spark.GlobalPosition = GlobalPosition + new Vector3(0f, 1f, 0f);
		if(spark.GetNodeOrNull<GpuParticles3D>("GPUParticles3D") is { } particles) {
			particles.Restart();
			particles.Emitting = true;
		}
	}

	private void UpdateMovementState() {
		if(StateMachine.CurrentState == State.Attacking) { return; }

		if(AI.AttackPressed && AttackCooldownTimer <= 0f) {
			if(EnemyType == EnemyType.RadiationCaster && Animator != null && !StaffAttackAnimation.Equals(default(StringName))) {
				Animator.SetAttackAnimation(StaffAttackAnimation);
				AttackCooldownTimer = RangedAttackCooldown;
				SpawnRadiationBolt();
			}

			StateMachine.TransitionTo(State.Attacking);
			return;
		}

		if(!IsOnFloor()) {
			StateMachine.TransitionTo(State.Falling);
		}
		else if(!AI.IsMoving) {
			StateMachine.TransitionTo(State.Idle);
		}
		else if(AI.SprintHeld) {
			StateMachine.TransitionTo(State.Sprinting);
		}
		else {
			StateMachine.TransitionTo(State.Walking);
		}
	}

	public override void OnAttackFinished() {
		if(EnemyType == EnemyType.RadiationCaster) {
			StateMachine.TransitionTo(State.Idle);
			return;
		}

		if(AttackTarget != null &&
			IsInstanceValid(AttackTarget) &&
			AttackTarget is IHealth healthTarget) {

			Log.Info($"Enemy attacking {AttackTarget.Name}");
			this.Attack(healthTarget);
		}

		AttackCooldownTimer = RangedAttackCooldown;

		StateMachine.TransitionTo(State.Idle);
	}

	private void SpawnRadiationBolt() {
		if(RadiationBoltScene?.Instantiate() is not RadiationBolt bolt) {
			return;
		}

		if(StaffCastPoint == null || AttackTarget == null || !IsInstanceValid(AttackTarget)) {
			bolt.QueueFree();
			return;
		}

		GetTree().CurrentScene?.AddChild(bolt);
		bolt.GlobalTransform = StaffCastPoint.GlobalTransform;
		bolt.SetSpeed(RangedProjectileSpeed);

		Vector3 targetPosition = AttackTarget.GlobalPosition + new Vector3(0f, 1.0f, 0f);
		Vector3 direction = (targetPosition - StaffCastPoint.GlobalPosition).Normalized();
		bolt.Init(this, direction, Offense.Damage);
	}

	public EnemyData Export() => new() {
		Health = Health.Export(),
		Movement = Movement.Export(),
		Offense = Offense.Export(),
		Defense = Defense.Export(),
	};

	public void Import(EnemyData data) {
		if(!string.IsNullOrEmpty(data.Id)) {
			Id = data.Id;
		}
		Health.Import(data.Health);
		Movement.Import(data.Movement);
		Offense.Import(data.Offense);
		Defense.Import(data.Defense);
	}
}

public readonly record struct EnemyData : ISaveData {
	public string Id { get; init; }
	public HealthData Health { get; init; }
	public MovementData Movement { get; init; }
	public OffenseData Offense { get; init; }
	public DefenseData Defense { get; init; }
}
