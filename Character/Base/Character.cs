using System;
using Components;
using Core;
using Godot;
using Services;

namespace Character {
	public abstract partial class CharacterBase : CharacterBody3D, IHealth, IAttack {
		[Export] private int InitialHealth = 100;
		[Export] private int InitialDamage = 10;

		public Health Health { get; }
		public Attack Attack { get; }

		public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dead }

		private readonly StateMachine<State> StateMachine = new(State.Idle);

		public State CurrentState => StateMachine.CurrentState;
		public event Action<State, State>? OnStateChanged;

		public CharacterBase() {
			Health = new Health(InitialHealth);
			Attack = new Attack(InitialDamage);

			StateMachine.OnChange((from, to) => OnStateChanged?.Invoke(from, to));
		}
	}
}