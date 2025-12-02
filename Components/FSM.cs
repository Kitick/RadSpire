using System;

public sealed class FiniteStateMachine<TState> where TState : Enum {
	public TState State { get; private set; }

	public event Action<TState, TState>? OnStateChanged;

	public FiniteStateMachine(TState initalState) => State = initalState;

	public void TransitionTo(TState newState) {
		if(State.Equals(newState)) { return; }

		var old = State;
		State = newState;

		OnStateChanged?.Invoke(old, newState);
	}
}