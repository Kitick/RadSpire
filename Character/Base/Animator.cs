namespace Character;

using Godot;
using Root;
using Services;
using CharState = CharacterBase.State;

public sealed partial class Animator : AnimationPlayer {
	private static readonly LogService Log = new(nameof(Animator), enabled: true);
	private const string StepSoundPath = "res://Assets/Audio/step.wav";
	private const string AttackSwingSoundPath = "res://Assets/Audio/ESM_FG2_FX_combat_one_shot_slash_animee_sword_hit_2.wav";

	private AudioStreamPlayer3D? StepPlayer;
	private AudioStreamPlayer3D? ActionPlayer;
	private AudioStream? StepSound;
	private AudioStream? AttackSwingSound;
	private double LastFootstepTime;

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
	[Export] private bool EnableHardcodedSfx = true;
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
		SetupSfx();
		Character.OnStateChanged += OnMovement;
		SetupAnimations();
		SyncAnimation(Character.CurrentState);
	}

	public override void _Process(double delta) {
		if(!EnableHardcodedSfx || !IsPlayerAnimator) {
			return;
		}

		double now = Time.GetTicksMsec() / 1000.0;
		float interval = Character.CurrentState == CharState.Sprinting
			? SprintStepIntervalSeconds
			: WalkStepIntervalSeconds;

		if((Character.CurrentState == CharState.Walking || Character.CurrentState == CharState.Sprinting) &&
			now - LastFootstepTime >= interval) {
			PlayFootstep();
		}
	}

	private void SetupSfx() {
		if(!EnableHardcodedSfx || !IsPlayerAnimator) {
			return;
		}

		StepSound = GD.Load<AudioStream>(StepSoundPath);
		AttackSwingSound = GD.Load<AudioStream>(AttackSwingSoundPath);

		if(StepSound == null) {
			Log.Warn($"Step sound not found at {StepSoundPath}");
		}

		if(AttackSwingSound == null) {
			Log.Warn($"Attack swing sound not found at {AttackSwingSoundPath}");
		}

		StepPlayer = new AudioStreamPlayer3D {
			Name = "PlayerStepSfx",
			Bus = "SFX",
			VolumeDb = -5.0f,
			MaxDistance = 20.0f,
			UnitSize = 1.0f,
			Stream = StepSound,
		};

		ActionPlayer = new AudioStreamPlayer3D {
			Name = "PlayerActionSfx",
			Bus = "SFX",
			VolumeDb = -4.0f,
			MaxDistance = 24.0f,
			UnitSize = 1.0f,
			Stream = AttackSwingSound,
		};

		AddChild(StepPlayer);
		AddChild(ActionPlayer);
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

	public void AnimEventFootstep() => PlayFootstep();

	public void AnimEventAttackSwing() {
		if(!EnableHardcodedSfx || !IsPlayerAnimator || ActionPlayer == null || AttackSwingSound == null) {
			return;
		}

		ActionPlayer.Stream = AttackSwingSound;
		ActionPlayer.PitchScale = (float) GD.RandRange(0.97, 1.05);
		ActionPlayer.Play();
	}

	public void AnimEventLand() {
		if(!EnableHardcodedSfx || !IsPlayerAnimator || StepPlayer == null || StepSound == null) {
			return;
		}

		StepPlayer.Stream = StepSound;
		StepPlayer.PitchScale = 0.82f;
		StepPlayer.VolumeDb = -1.5f;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}

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
			PlayFootstep(0.90f, -3.0f);
			PlayingAnimation = AnimState.Jumping;
		}
		else if(landed) {
			AnimEventLand();
			PlayingAnimation = AnimState.Landing;
		}
		else { SyncAnimation(to); }
	}

	private void PlayFootstep(float pitch = 1.0f, float volumeDb = -5.0f) {
		if(!EnableHardcodedSfx || !IsPlayerAnimator || StepPlayer == null || StepSound == null) {
			return;
		}

		StepPlayer.Stream = StepSound;
		StepPlayer.PitchScale = pitch + (float) GD.RandRange(-0.03, 0.03);
		StepPlayer.VolumeDb = volumeDb;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}
}
