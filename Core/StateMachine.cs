using System;
using System.Collections.Generic;

namespace Core {
	public sealed class StateMachine<TState> where TState : struct, Enum {
		private readonly Dictionary<(TState? from, TState? to), Action> Transitions = [];

		private TState? State;
		public TState CurrentState => State ?? throw new InvalidOperationException("StateMachine is not in a state.");

		public StateMachine(TState? initial = null) => State = initial;

		private Action? Get(TState? from, TState? to) {
			return Transitions.TryGetValue((from, to), out var action) ? action : null;
		}

		private void Register(Action action, TState? from = null, TState? to = null) {
			var key = (from, to);

			if(Transitions.TryGetValue(key, out var existing)) {
				Transitions[key] = existing + action;
			}
			else {
				Transitions[key] = action;
			}
		}

		private Action? GetSpecific(TState from, TState to) => Get(from, to);
		private Action? GetEnter(TState state) => Get(null, state);
		private Action? GetExit(TState state) => Get(state, null);
		private Action? GetAny() => Get(null, null);

		public void OnSpecific(TState from, TState to, Action action) => Register(action, from, to);
		public void OnEnter(TState state, Action action) => Register(action, null, state);
		public void OnExit(TState state, Action action) => Register(action, state, null);
		public void OnAny(Action action) => Register(action, null, null);

		public void TransitionTo(TState next) {
			TState? prev = State;
			State = null;

			GetAny()?.Invoke();
			if(prev is not null) { GetExit(prev.Value)?.Invoke(); }
			GetEnter(next)?.Invoke();
			if(prev is not null) { GetSpecific(prev.Value, next)?.Invoke(); }

			State = next;
		}
	}
}