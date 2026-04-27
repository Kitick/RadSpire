namespace Character;

using System;

using Godot;
using Root;
using Services;
using CharState = CharacterBase.State;

public sealed partial class Animator : AnimationPlayer {
	private static readonly LogService Log = new(nameof(Animator), enabled: false);

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
	[Export] private StringName DODGE = null!;
	[Export] private StringName[] AttackVariations = [];
	[Export] private bool CycleAttackVariations = false;

	[ExportCategory("Animation Settings")]
	[Export] private float SprintSpeed = 1.0f;
	[Export] private float WalkStepIntervalSeconds = 0.40f;
	[Export] private float SprintStepIntervalSeconds = 0.24f;

	private bool IsPlayerAnimator => Character is Player;
	[Export] private float AttackSpeed = 1.0f;
	[Export] private float AttackBlend = 0.02f;
	[Export] private float DodgeSpeed = 1.6f;
	[Export] private float DodgeBlend = 0.2f;
	[Export] private float DodgeToMoveBlend = 0.25f;
	[Export] private float DodgeIdleRecoveryTime = 0.15f;
	private bool UseDodgeIdleRecovery = true;

	public void SetAttackSpeed(float speed) => AttackSpeed = Math.Max(0.1f, speed);

	public void SetDodgeIdleRecovery(bool enabled) => UseDodgeIdleRecovery = enabled;

	public enum AnimState { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, Attacking, Dying, Dodging }

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
				case AnimState.Dodging: Play(DODGE, DodgeBlend, DodgeSpeed); break;
				case AnimState.Attacking: Play(GetAttackAnimation(), AttackBlend, AttackSpeed); break;
				case AnimState.Dying: Play(DEATH); break;
			}
		}
	}

	private int AttackVariationIndex = 0;

	public void SetDodgeAnimation(StringName name) {
		DODGE = name;
		ApplyDodgeBlendTimes();
	}

	public override void _Ready() {
		this.ValidateExports();

		// Manually resolve Character if not already assigned
		if(Character == null) {
			if(GetParent()?.GetParent() is not CharacterBase resolved) {
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
		if(Character is not Player player) { return; }

		Audio = new PlayerAudio {
			WalkStepIntervalSeconds = WalkStepIntervalSeconds,
			SprintStepIntervalSeconds = SprintStepIntervalSeconds,
		};
		AddChild(Audio);
		CallDeferred(nameof(SetupPlayerAudioDeferred), player);
	}

	private void SetupPlayerAudioDeferred(Player player) => Audio?.Setup(player);

	private void SetupAnimations() {
		SetLoopMode(IDLE);
		SetLoopMode(WALKING);
		SetLoopMode(SPRINTING);
		SetLoopMode(CROUCHING);
		SetLoopMode(FALLING);

		ApplyDodgeBlendTimes();

		AnimationFinished += OnAnimationFinished;
	}

	private void SetLoopMode(StringName name) => GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;

	public void OnAnimationFinished(StringName name) {
		if(name == JUMPING || name == LANDING) { SyncAnimation(Character.CurrentState); } else if(name == DODGE) { _ = StartDodgeIdleRecovery(); } else if(IsAttackAnimation(name)) { Character.OnAttackFinished(); }
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
			CharState.Dodging => AnimState.Dodging,
			CharState.Dead => AnimState.Dying,
			_ => PlayingAnimation,
		};
	}

	private void OnMovement(CharState from, CharState to) {
		Log.Info($"Character state change: {from} -> {to}");

		bool jumped = from != CharState.Falling && to == CharState.Falling;
		bool landed = from == CharState.Falling && to != CharState.Falling;

		if(jumped) {
			Audio?.PlayFootstep(0.90f, -10.0f);
			PlayingAnimation = AnimState.Jumping;
		}
		else if(landed) {
			Audio?.PlayLand();
			PlayingAnimation = AnimState.Landing;
		}
		else { SyncAnimation(to); }
	}

	private bool IsAttackAnimation(StringName name) {
		if(name == ATTACK) { return true; }
		for(int i = 0; i < AttackVariations.Length; i++) {
			if(AttackVariations[i] == name) { return true; }
		}
		return false;
	}

	private StringName GetAttackAnimation() {
		if(AttackVariations.Length == 0) { return ATTACK; }
		if(!CycleAttackVariations) {
			int idx = GD.RandRange(0, AttackVariations.Length - 1);
			return AttackVariations[idx];
		}

		if(AttackVariationIndex < 0 || AttackVariationIndex >= AttackVariations.Length) {
			AttackVariationIndex = 0;
		}

		StringName picked = AttackVariations[AttackVariationIndex];
		AttackVariationIndex = (AttackVariationIndex + 1) % AttackVariations.Length;
		return picked;
	}

	private StringName CurrentDodgeAnimation() => DODGE;

	private async System.Threading.Tasks.Task StartDodgeIdleRecovery() {
		if(!UseDodgeIdleRecovery || DodgeIdleRecoveryTime <= 0f) {
			Character.OnDodgeFinished();
			return;
		}

		// Briefly play idle as a recovery pose to reduce snapping.
		Play(IDLE);
		if(DodgeIdleRecoveryTime > 0f) {
			SceneTreeTimer timer = GetTree().CreateTimer(DodgeIdleRecoveryTime);
			await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
		}
		Character.OnDodgeFinished();
	}

	private void ApplyDodgeBlendTimes() {
		if(DODGE == null) { return; }
		SetBlendTime(DODGE, IDLE, 0.4f);
		SetBlendTime(DODGE, WALKING, 0.4f);
		SetBlendTime(DODGE, SPRINTING, DodgeToMoveBlend);
		SetBlendTime(DODGE, CROUCHING, DodgeToMoveBlend);
	}
}
