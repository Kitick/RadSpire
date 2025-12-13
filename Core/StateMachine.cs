//This file was developed entirely by the RadSpire Development Team.

using System;

public sealed class FiniteStateMachine<TState> where TState : Enum {
	public TState State { get; private set; }

	public event Action<TState, TState>? OnStateChanged;

	public FiniteStateMachine(TState inital) => State = inital;
	public FiniteStateMachine(TState inital, Action<TState, TState> onChanged) : this(inital) => OnStateChanged += onChanged;

	public void TransitionTo(TState newState) {
		if(State.Equals(newState)) { return; }

		var last = State;
		State = newState;

		OnStateChanged?.Invoke(last, newState);
	}
}