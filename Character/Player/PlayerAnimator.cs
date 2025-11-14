using System;
using Godot;

public partial class PlayerAnimator : Node3D {
	public enum State { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing }

	private const string IDLE = "Idle";
	private const string WALKING = "Walking_B";
	private const string SPRINTING = "Running_B";
	private const string CROUCHING = "Walking_C";
	private const string JUMPING = "Jump_Start";
	private const string FALLING = "Jump_Idle";
	private const string LANDING = "Jump_Land";

	private const string ANIMATION_PLAYER = "AnimationPlayer";

	private Player Player = null!;
	private AnimationPlayer AnimationPlayer = null!;

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
			}
		}
	}

	public override void _Ready() {
		GetComponents();
		SetupAnimations();
	}

	private void GetComponents() {
		Player = GetParent<Player>();
		Player.OnStateChange += OnPlayerMovement;

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
	}

	public void SyncAnimation() {
		GD.Print($"Syncing animation to: {Player.CurrentState}");

		CurrentAnimation = Player.CurrentState switch {
			Player.State.Idle => State.Idle,
			Player.State.Walking => State.Walking,
			Player.State.Sprinting => State.Sprinting,
			Player.State.Crouching => State.Crouching,
			Player.State.Falling => State.Falling,
			_ => CurrentAnimation,
		};
	}

	private void OnPlayerMovement(Player.State from, Player.State to) {
		GD.Print($"Player State change: {from} -> {to}");

		bool jumped = from != Player.State.Falling && to == Player.State.Falling;
		bool landed = from == Player.State.Falling && to != Player.State.Falling;

		if(jumped) { CurrentAnimation = State.Jumping; }
		else if(landed) { CurrentAnimation = State.Landing; }
		else { SyncAnimation(); }
	}
}