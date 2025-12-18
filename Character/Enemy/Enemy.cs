using Components;
using Core;
using Godot;
using Root;
using Services;

namespace Character {
	public sealed partial class Enemy : CharacterBody3D, IHealth, IAttack, ISaveable<EnemyData> {
		private static readonly LogService Log = new(nameof(Enemy), enabled: true);

		[Export] private int InitialHealth = 50;
		[Export] private int InitialDamage = 5;

		private Player? Target;

		// Components

		public Health Health { get; }
		public Attack Attack { get; }

		private readonly Movement Movement;
		private readonly ChaseAI AI;

		// State Machine

		public enum State { Idle, Walking, Sprinting, Crouching, Falling }

		private readonly StateMachine<State> StateMachine = new(State.Idle);
		public State CurrentState => StateMachine.CurrentState;

		public Enemy() {
			Movement = new Movement(this);
			AI = new ChaseAI(this);

			Health = new Health(InitialHealth);
			Attack = new Attack(InitialDamage);
		}

		public override void _Ready() {

		}

		public override void _PhysicsProcess(double delta) {
			float dt = (float) delta;

			AI.Update();

			Movement.Move(AI.HorizontalInput, 1);

			Movement.Update(dt);
		}

		public EnemyData Serialize() => new EnemyData {
			Health = Health.Serialize(),
			Movement = Movement.Serialize(),
		};

		public void Deserialize(in EnemyData data) {
			Health.Deserialize(data.Health);
			Movement.Deserialize(data.Movement);
		}
	}

	public readonly record struct EnemyData : ISaveData {
		public HealthData Health { get; init; }
		public MovementData Movement { get; init; }
	}
}
