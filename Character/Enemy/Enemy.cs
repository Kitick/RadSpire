namespace Character {
using Components;
using Core;
using Godot;
using Services;
	public sealed partial class Enemy : CharacterBase, ISaveable<EnemyData> {
		private static readonly LogService Log = new(nameof(Enemy), enabled: true);

		[Export] private int InitialHealthValue = 50;
		[Export] private int InitialDamagePhysical = 5;
		[Export] private int InitialDamageMagic = 0;
		[Export] private int InitialDefensePhysical = 0;
		[Export] private int InitialDefenseMagic = 0;

		protected override int InitialHealth => InitialHealthValue;
		protected override (int phys, int mag) InitialDamage => (InitialDamagePhysical, InitialDamageMagic);
		protected override (int phys, int mag) InitialDefense => (InitialDefensePhysical, InitialDefenseMagic);

		private Player? Target;

		// Components
		private readonly Movement Movement;
		private readonly ChaseAI AI;

		public Enemy() {
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
			if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
			else if(!AI.IsMoving) { StateMachine.TransitionTo(State.Idle); }
			else if(AI.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
			else { StateMachine.TransitionTo(State.Walking); }
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
}
