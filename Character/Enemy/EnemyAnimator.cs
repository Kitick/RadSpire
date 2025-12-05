using System;
using Godot;

public sealed partial class EnemyAnimator : Node3D {
	public bool Debug = false;

	public enum State { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, Die }

	private const string IDLE = "Idle";
	private const string WALKING = "Walking_B";
	private const string SPRINTING = "Running_B";
	private const string CROUCHING = "Walking_C";
	private const string JUMPING = "Jump_Start";
	private const string FALLING = "Jump_Idle";
	private const string LANDING = "Jump_Land";
	private const string DIE = "Death_A";
	private const string ANIMATION_PLAYER = "AnimationPlayer";

	private Enemy Enemy = null!;
	private AnimationPlayer AnimationPlayer = null!;
	private bool IsDying;
	public State CurrentAnimation {
		get;
		private set {
			field = value;
			switch(value) {
				case State.Idle: AnimationPlayer.Play(IDLE); break;
				case State.Walking: AnimationPlayer.Play(WALKING); break;
				case State.Sprinting: AnimationPlayer.Play(SPRINTING); break;
				case State.Crouching: AnimationPlayer.Play(CROUCHING); break;
				case State.Jumping: AnimationPlayer.Play(JUMPING); break;
				case State.Falling: AnimationPlayer.Play(FALLING); break;
				case State.Landing: AnimationPlayer.Play(LANDING); break;
				case State.Die: AnimationPlayer.Play(DIE); break;
			}
		}
	}

	public override void _Ready() {
		GetComponents();
		SetupAnimations();
	}

	private void GetComponents() {
		Enemy = GetParent<Enemy>();
		Enemy.OnStateChange += OnEnemyMovement;

		AnimationPlayer = GetNode<AnimationPlayer>(ANIMATION_PLAYER);
	}

	private void SetupAnimations() {
		SetLoopMode(IDLE);
		SetLoopMode(WALKING);
		SetLoopMode(SPRINTING);
		SetLoopMode(CROUCHING);
		SetLoopMode(FALLING);

		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		SyncAnimation();
	}

	private void SetLoopMode(string name) {
		AnimationPlayer.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
	}

	public void OnAnimationFinished(StringName name) {
		if(name == JUMPING || name == LANDING) {
			SyncAnimation();
		}
		else if (name == DIE) {
			GetParent<Enemy>().Die();
			SyncAnimation();
		}
	}

	public void SyncAnimation() {
		if(Debug){ GD.Print($"Syncing animation to: {Enemy.CurrentState}"); }
		
		if (IsDying)
			return;

		CurrentAnimation = Enemy.CurrentState switch {
			Enemy.State.Idle => State.Idle,
			Enemy.State.Walking => State.Walking,
			Enemy.State.Sprinting => State.Sprinting,
			Enemy.State.Crouching => State.Crouching,
			Enemy.State.Falling => State.Falling,
			_ => CurrentAnimation,
		};
	}
	
	public void PlayDie()
	{
		IsDying = true;
		AnimationPlayer.Play(DIE);
	}

	private void OnEnemyMovement(Enemy.State from, Enemy.State to) {
		if(Debug){ GD.Print($"Enemy State change: {from} -> {to}"); }
		
		if (IsDying)
			return;

		bool jumped = from != Enemy.State.Falling && to == Enemy.State.Falling;
		bool landed = from == Enemy.State.Falling && to != Enemy.State.Falling;

		if(jumped) { CurrentAnimation = State.Jumping; }
		else if(landed) { CurrentAnimation = State.Landing; }
		else { SyncAnimation(); }
	}
}