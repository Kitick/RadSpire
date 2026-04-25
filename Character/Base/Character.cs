namespace Character;

using System;
using Components;
using Godot;
using Root;

public abstract partial class CharacterBase : CharacterBody3D, IHealth, IOffense, IDefense {
	protected abstract int InitialHealth { get; }
	protected abstract int InitialDamage { get; }
	protected abstract int InitialDefense { get; }

	public Health Health { get; private set; } = null!;
	public Offense Offense { get; private set; } = null!;
	public Defense Defense { get; private set; } = null!;

	public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dead }

	protected readonly StateMachine<State> StateMachine = new(State.Idle);

	public State CurrentState => StateMachine.CurrentState;
	public event Action<State, State>? OnStateChanged;

	public virtual void OnAttackFinished() { }

	public override void _Ready() {
		Health = new Health(InitialHealth);
		Offense = new Offense(InitialDamage);
		Defense = new Defense(InitialDefense);

		StateMachine.OnChange((from, to) => OnStateChanged?.Invoke(from, to));
	}
}
