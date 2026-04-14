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

	[Export] public string EnemyGroup { get; set; } = "";

	[Export] private float KnockbackForce = 15f;
	[Export] private float KnockbackDecay = 12f;
	[Export] private float DamageFlashTime = 0.5f;

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
	private MeshInstance3D? EnemyMesh;
	private StandardMaterial3D? FlashMaterial;

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

		if(EnemyMesh != null) {
			var baseMat = EnemyMesh.GetActiveMaterial(0) as StandardMaterial3D;
			if(baseMat != null) {
				FlashMaterial = baseMat.Duplicate() as StandardMaterial3D;
				EnemyMesh.SetSurfaceOverrideMaterial(0, FlashMaterial);
			}
		}

		Health.OnChanged += (from, to) => {
			if(to.Current >= from.Current) {
				return;
			}

			DamageFlashTimer = DamageFlashTime;
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

		AI.Update();
		Movement.Move(AI.HorizontalInput, 1);
		Movement.Update(dt);

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
