namespace Character;

using Godot;
using Root;
using Services;
using CharState = CharacterBase.State;

public sealed partial class Animator : AnimationPlayer {
	private static readonly LogService Log = new(nameof(Animator), enabled: true);

	private PlayerAudio? Audio;

	[ExportCategory("References")]
	[Export] private CharacterBase Character = null!;

	[ExportCategory("Animation Names")]
	[Export] private StringName IDLE = null!;
	[Export] private StringName WALKING = null!;
	[Export] private StringName SPRINTING = null!;
	[Export] private StringName CROUCHING = null!;
	[Export] private StringName JUMPING = null!;
	[Export] private StringName FALLING = null!;
	[Export] private StringName LANDING = null!;
	[Export] private StringName ATTACK = null!;
	[Export] private StringName DEATH = null!;

	[ExportCategory("Animation Settings")]
	[Export] private float SprintSpeed = 1.0f;
	[Export] private float WalkStepIntervalSeconds = 0.40f;
	[Export] private float SprintStepIntervalSeconds = 0.24f;

	public enum AnimState { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, Attacking, Dying }

	private bool IsPlayerAnimator => Character is Player;

	private AnimState PlayingAnimation {
		get;
		set {
			field = value;
			switch(value) {
				case AnimState.Idle: Play(IDLE); break;
				case AnimState.Walking: Play(WALKING); break;
				case AnimState.Sprinting: Play(SPRINTING, 0.1f, SprintSpeed); break;
				case AnimState.Crouching: Play(CROUCHING); break;
				case AnimState.Jumping: Play(JUMPING); break;
				case AnimState.Falling: Play(FALLING); break;
				case AnimState.Landing: Play(LANDING); break;
				case AnimState.Attacking: Play(ATTACK); break;
				case AnimState.Dying: Play(DEATH); break;
			}
		}
	}

	public override void _Ready() {
		this.ValidateExports();
		
		// Manually resolve Character if not already assigned
		if(Character == null) {
			CharacterBase? resolved = GetParent()?.GetParent() as CharacterBase;
			if(resolved == null) {
				Log.Error("Failed to resolve Character reference for Animator!");
				return;
			}
			Character = resolved;
		}
		
		SetupSfx();
		Character.OnStateChanged += OnMovement;
		SetupAnimations();
		SyncAnimation(Character.CurrentState);
	}

	public override void _Process(double delta) {
		if(Audio == null) { return; }

		Audio.ProcessFootsteps(Character.CurrentState, Time.GetTicksMsec() / 1000.0);
	}

	private void SetupSfx() {
		if(!IsPlayerAnimator) { return; }

		Audio = new PlayerAudio {
			WalkStepIntervalSeconds = WalkStepIntervalSeconds,
			SprintStepIntervalSeconds = SprintStepIntervalSeconds,
		};
		AddChild(Audio);
		Audio.Setup();
	}

	private void SetupAnimations() {
		SetLoopMode(IDLE);
		SetLoopMode(WALKING);
		SetLoopMode(SPRINTING);
		SetLoopMode(CROUCHING);
		SetLoopMode(FALLING);

		AnimationFinished += OnAnimationFinished;
	}

	private void SetLoopMode(StringName name) {
		GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
	}

	public void OnAnimationFinished(StringName name) {
		if(name == JUMPING || name == LANDING) { SyncAnimation(Character.CurrentState); }
		else if(name == ATTACK) { Character.OnAttackFinished(); }
	}

	public void AnimEventFootstep() => Audio?.PlayFootstep();

	public void AnimEventLand() => Audio?.PlayLand();

	public void SyncAnimation(CharState state) {
		Log.Info($"Syncing animation to: {state}");

		PlayingAnimation = state switch {
			CharState.Idle => AnimState.Idle,
			CharState.Walking => AnimState.Walking,
			CharState.Sprinting => AnimState.Sprinting,
			CharState.Crouching => AnimState.Crouching,
			CharState.Falling => AnimState.Falling,
			CharState.Attacking => AnimState.Attacking,
			CharState.Dead => AnimState.Dying,
			_ => PlayingAnimation,
		};
	}

	private void OnMovement(CharState from, CharState to) {
		Log.Info($"Character state change: {from} -> {to}");

		bool jumped = from != CharState.Falling && to == CharState.Falling;
		bool landed = from == CharState.Falling && to != CharState.Falling;

		if(jumped) {
			Audio?.PlayFootstep(0.90f, -3.0f);
			PlayingAnimation = AnimState.Jumping;
		}
		else if(landed) {
			Audio?.PlayLand();
			PlayingAnimation = AnimState.Landing;
		}
		else { SyncAnimation(to); }
	}
}