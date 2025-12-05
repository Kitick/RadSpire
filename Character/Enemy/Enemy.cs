using System;
using Components;
using Core;
using Godot;
using SaveSystem;

public sealed partial class Enemy : CharacterBody3D, ISaveable<EnemyData> {
	[Export] private int InitalHealth = 100;
	[Export] private float SprintMultiplier = 1.5f;
	[Export] private float CrouchMultiplier = 0.5f;
	[Export] public Node3D Player;
	

	// Components
	private Health Health = null!;
	private Movement Movement = null!;
	private AiInput AiInput = null!;


	// State Machine
	public enum State { Idle, Walking, Sprinting, Crouching, Falling }

	private readonly FiniteStateMachine<State> StateMachine = new(State.Idle);
	public State CurrentState => StateMachine.State;

	public event Action<State, State>? OnStateChange {
		add => StateMachine.OnStateChanged += value;
		remove => StateMachine.OnStateChanged -= value;
	}

	public Enemy() {
		AiInput = new AiInput(this, Player);
	}

	public override void _Ready() {
		Movement = new Movement(this);
		Health = new Health(InitalHealth);

		Node3D? player = null;
		
		var players = GetTree().GetNodesInGroup("Player");
		if (players.Count > 0) {
			player = players[0] as Node3D;
		}
		
		if (player == null) {
			GD.PushWarning("Enemy could not find any node in group 'player'. AI will not move.");
			return;
		}

		AiInput = new AiInput(this, player);
		
	}
	
	public void TakeDamage(int amount)
	{
		Health.CurrentHealth -= amount;
		GD.Print($"Enemy HP: {Health.CurrentHealth}");

		if (Health.IsDead())
			Die();
	}

	private void Die()
	{
		QueueFree();
	}

	public override void _PhysicsProcess(double delta) {
		if(Health.IsDead()) {
			Die();
			return;
		}

		float dt = (float) delta;
		
		AiInput.Update();
		
		float multiplier = GetMultiplier();
		
		if(IsOnFloor()) {
			Movement.Move(AiInput.HorizontalInput, multiplier);
		}

		Movement.Update(dt);

		UpdateMovementState();
	}

	private void UpdateMovementState() {
		if(!IsOnFloor()) { StateMachine.TransitionTo(State.Falling); }
		else if(!AiInput.IsMoving) { StateMachine.TransitionTo(State.Idle); }
		else if(AiInput.SprintHeld) { StateMachine.TransitionTo(State.Sprinting); }
		else if(AiInput.CrouchHeld) { StateMachine.TransitionTo(State.Crouching); }
		else { StateMachine.TransitionTo(State.Walking); }
	}

	private float GetMultiplier() {
		float multiplier = 1.0f;

		if(StateMachine.State == State.Sprinting) { multiplier *= SprintMultiplier; }
		if(StateMachine.State == State.Crouching) { multiplier *= CrouchMultiplier; }

		return multiplier;
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

namespace SaveSystem {
	public readonly record struct EnemyData : ISaveData {
		public HealthData Health { get; init; }
		public MovementData Movement { get; init; }
	}
}
