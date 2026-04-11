using System;

namespace Character;

using Components;
using Godot;
using Services;

public sealed partial class Enemy : CharacterBase, ISaveable<EnemyData> {

	private static readonly LogService Log = new(nameof(Enemy), enabled: true);

	[Export] private int InitialHealthValue = 50;
	[Export] private int InitialDamagePhysical = 5;
	[Export] private int InitialDamageMagic = 0;
	[Export] private int InitialDefensePhysical = 0;
	[Export] private int InitialDefenseMagic = 0;

	[Export] private float KnockbackForce = 15f;
	[Export] private float KnockbackDecay = 12f;
	[Export] private float DamageFlashTime = 1.0f;
	[Export] private float StunDuration = 0.4f;
	[Export] private float HealthBarWidth = 1.4f;
	[Export] private float HealthBarHeight = 0.12f;
	[Export] private float HealthBarYOffset = 2.2f;
	[Export] private Color HealthBarFillColor = new(0.2f, 0.9f, 0.2f);
	[Export] private Color HealthBarBackColor = new(0f, 0f, 0f, 0.6f);
	[Export] private float DamageNumberLifetime = 0.9f;
	[Export] private float DamageNumberRise = 0.6f;
	[Export] private float DamageNumberHorizontalJitter = 0.25f;
	[Export] private Color DamageNumberColor = new(1f, 0.85f, 0.2f);
	[Export] private int DamageNumberFontSize = 55;

	protected override int InitialHealth => InitialHealthValue;
	protected override (int phys, int mag) InitialDamage => (InitialDamagePhysical, InitialDamageMagic);
	protected override (int phys, int mag) InitialDefense => (InitialDefensePhysical, InitialDefenseMagic);

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

		EnemyMesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		var animator = GetNodeOrNull<Animator>("Model/AnimationPlayer");
		if(animator != null) {
			animator.SetAttackSpeed(1.5f);
		}
		SetupHealthUI();

		if(EnemyMesh != null) {
			var baseMat = EnemyMesh.GetActiveMaterial(0) as StandardMaterial3D;
			if(baseMat != null) {
				FlashMaterial = baseMat.Duplicate() as StandardMaterial3D;
				EnemyMesh.SetSurfaceOverrideMaterial(0, FlashMaterial);
			}
		}

		Health.OnChanged += (from, to) => {
			int damageTaken = from.Current - to.Current;
			if(damageTaken > 0) {
				SpawnDamageNumber(damageTaken);
			}

			UpdateHealthUI();
			if(to.Current >= from.Current) {
				return;
			}

			DamageFlashTimer = DamageFlashTime;
			StunTimer = Math.Max(StunTimer, StunDuration);
			SetDamageFlash(true);

			if(AttackTarget == null || !GodotObject.IsInstanceValid(AttackTarget)) {
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

		if(StunTimer > 0f) {
			StunTimer -= dt;
		}
		else {
			AI.Update();
			Movement.Move(AI.HorizontalInput, 1);
			Movement.Update(dt);
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
			var image = Image.Create(1, 1, false, Image.Format.Rgba8);
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

		var scale = HealthBarFill.Scale;
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

		if(HealthUIRoot == null) {
			HealthUIRoot = new Node3D { Name = "HealthUI" };
			AddChild(HealthUIRoot);
		}

		var label = new Label3D {
			Text = $"-{amount}",
			FontSize = DamageNumberFontSize,
			Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
			Modulate = DamageNumberColor
		};

		float jitter = (float) GD.RandRange(-DamageNumberHorizontalJitter, DamageNumberHorizontalJitter);
		label.Position = new Vector3(jitter, HealthBarYOffset + 0.25f, 0f);
		HealthUIRoot.AddChild(label);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(label, "position",
			label.Position + new Vector3(0f, DamageNumberRise, 0f),
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

	private void UpdateMovementState() {
		if(StateMachine.CurrentState == State.Attacking) { return; }

		if(AI.AttackPressed) {
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
		if(AttackTarget != null &&
			GodotObject.IsInstanceValid(AttackTarget) &&
			AttackTarget is IHealth healthTarget) {

			Log.Info($"Enemy attacking {AttackTarget.Name}");
			this.Attack(healthTarget);
		}

		StateMachine.TransitionTo(State.Idle);
	}

	public EnemyData Export() => new EnemyData {
		Health = Health.Export(),
		Movement = Movement.Export(),
		Offense = Offense.Export(),
		Defense = Defense.Export(),
	};

	public void Import(EnemyData data) {
		Health.Import(data.Health);
		Movement.Import(data.Movement);
		Offense.Import(data.Offense);
		Defense.Import(data.Defense);
	}
}

public readonly record struct EnemyData : ISaveData {
	public HealthData Health { get; init; }
	public MovementData Movement { get; init; }
	public OffenseData Offense { get; init; }
	public DefenseData Defense { get; init; }
}
