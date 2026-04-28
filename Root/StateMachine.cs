namespace Root;

using System;
using System.Collections.Generic;

public sealed class StateMachine<TState> where TState : struct, Enum {
	private readonly Dictionary<(TState? from, TState? to), Action<TState, TState>> Transitions = [];

	private TState? State;

	public TState CurrentState => State ?? throw new InvalidOperationException("StateMachine is not in a valid state.");
	public bool IsSettled => State != null;

	public StateMachine(TState? initial = null) => State = initial;

	private Action<TState, TState>? Get(TState? from, TState? to) => Transitions.TryGetValue((from, to), out Action<TState, TState>? action) ? action : null;

	private void Register(Action<TState, TState> action, TState? from = null, TState? to = null) {
		(TState? from, TState? to) key = (from, to);

		Transitions[key] = Transitions.TryGetValue(key, out Action<TState, TState>? existing) ? existing + action : action;
	}

	private Action<TState, TState>? GetSpecific(TState from, TState to) => Get(from, to);
	private Action<TState, TState>? GetEnter(TState state) => Get(null, state);
	private Action<TState, TState>? GetExit(TState state) => Get(state, null);
	private Action<TState, TState>? GetChange() => Get(null, null);

	public void OnSpecific(TState from, TState to, Action action) => Register((_, _) => action(), from, to);
	public void OnEnter(TState state, Action action) => Register((_, _) => action(), null, state);
	public void OnExit(TState state, Action action) => Register((_, _) => action(), state, null);
	public void OnChange(Action<TState, TState> action) => Register(action, null, null);

	public void Start(TState initial) {
		if(IsSettled) { throw new InvalidOperationException("StateMachine has already been started."); }

		GetEnter(initial)?.Invoke(initial, initial);

		State = initial;
	}

	public void ForceTransitionTo(TState next) {
		TState prev = CurrentState;
		State = null;

		GetChange()?.Invoke(prev, next);
		GetExit(prev)?.Invoke(prev, next);
		GetEnter(next)?.Invoke(prev, next);
		GetSpecific(prev, next)?.Invoke(prev, next);

		State = next;
	}

	public bool TransitionTo(TState next) {
		bool changed = !CurrentState.Equals(next);
		if(changed) { ForceTransitionTo(next); }
		return changed;
	}
}
