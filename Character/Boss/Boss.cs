namespace Character;

using Components;
using Godot;
using Services;

public sealed partial class Boss : CharacterBase, ISaveable<BossData> {
	private static readonly LogService Log = new(nameof(Boss), enabled: true);

	[Export] private int InitialHealthValue = 150;
	[Export] private int InitialDamagePhysical = 15;
	[Export] private int InitialDamageMagic = 0;
	[Export] private int InitialDefensePhysical = 0;
	[Export] private int InitialDefenseMagic = 0;

	protected override int InitialHealth => InitialHealthValue;
	protected override (int phys, int mag) InitialDamage => (InitialDamagePhysical, InitialDamageMagic);
	protected override (int phys, int mag) InitialDefense => (InitialDefensePhysical, InitialDefenseMagic);

	// Components
	private readonly Movement Movement;
	private readonly ChaseAI AI;
	private Node3D? AttackTarget;

	public void SetTarget(Node3D target) {
		AI.SetTarget(target);
		AttackTarget = target;
	}

	public Boss() {
		Movement = new Movement(this);
		AI = new ChaseAI(this);
	}

	public override void _Ready() {
		base._Ready();
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float) delta;

		if(this.IsDead()) {
			StateMachine.TransitionTo(State.Dead);
			return;
		}

		AI.Update();

		Movement.Move(AI.HorizontalInput, 1);

		Movement.Update(dt);

		UpdateMovementState();
	}

	private void UpdateMovementState() {
		if(StateMachine.CurrentState == State.Attacking) { return; }
		if(AI.AttackPressed) { StateMachine.TransitionTo(State.Attacking); return; }
		if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
		else if(!AI.IsMoving) { StateMachine.TransitionTo(State.Idle); }
		else if(AI.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
		else { StateMachine.TransitionTo(State.Walking); }
	}

	public override void OnAttackFinished() {
		if(AttackTarget != null && GodotObject.IsInstanceValid(AttackTarget) && AttackTarget is IHealth healthTarget) {
			Log.Info($"Boss attacking {AttackTarget.Name}");
			this.Attack(healthTarget);
		}
		StateMachine.TransitionTo(State.Idle);
	}

	public BossData Export() => new BossData {
		Health = Health.Export(),
		Movement = Movement.Export(),
		Offense = Offense.Export(),
		Defense = Defense.Export(),
	};

	public void Import(BossData data) {
		Health.Import(data.Health);
		Movement.Import(data.Movement);
		Offense.Import(data.Offense);
		Defense.Import(data.Defense);
	}
}

public readonly record struct BossData : ISaveData {
	public HealthData Health { get; init; }
	public MovementData Movement { get; init; }
	public OffenseData Offense { get; init; }
	public DefenseData Defense { get; init; }
}
