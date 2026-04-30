namespace Character;

using System;
using Components;
using Godot;
using Root;

public abstract partial class CharacterBase : CharacterBody3D, IHealth, IOffense, IDefense {
	protected abstract int InitialHealth { get; }
	protected abstract int InitialDamage { get; }
	protected abstract int InitialDefense { get; }

	private Health? _health;
	private Offense? _offense;
	private Defense? _defense;

	public Health Health => _health ??= new Health(InitialHealth);
	public Offense Offense => _offense ??= new Offense(InitialDamage);
	public Defense Defense => _defense ??= new Defense(InitialDefense);

	public enum State { Idle, Walking, Sprinting, Crouching, Falling, Attacking, Dodging, Blocking, Hit, Dead }

	protected readonly StateMachine<State> StateMachine = new(State.Idle);

	public State CurrentState => StateMachine.CurrentState;
	public event Action<State, State>? OnStateChanged;

	public virtual void OnAttackFinished() { }
	public virtual void OnDodgeFinished() { }
	public virtual void OnHitFinished() { }
	public virtual bool ShouldLoopBlockingAnimation() => false;

	public override void _Ready() {
		StateMachine.OnChange((from, to) => OnStateChanged?.Invoke(from, to));
	}
}
