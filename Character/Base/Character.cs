using System;
using Components;
using Core;
using Godot;

namespace Character {
	public abstract partial class CharacterBase : CharacterBody3D, IHealth, IOffense, IDefense {
		protected abstract int InitialHealth { get; }
		protected abstract (int phys, int mag) InitialDamage { get; }
		protected abstract (int phys, int mag) InitialDefense { get; }

		public Health Health { get; protected set; } = null!;
		public Offense Offense { get; protected set; } = null!;
		public Defense Defense { get; protected set; } = null!;

		protected readonly StateMachine<State> StateMachine = new(State.Idle);

		public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dead }
		public State CurrentState => StateMachine.CurrentState;
		public event Action<State, State>? OnStateChanged;

		public override void _Ready() {
			Health = new Health(InitialHealth);
			Offense = new Offense(InitialDamage.phys, InitialDamage.mag);
			Defense = new Defense(InitialDefense.phys, InitialDefense.mag);

			StateMachine.OnChange((from, to) => OnStateChanged?.Invoke(from, to));
		}
	}
}