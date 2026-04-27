namespace Character;

using Components;
using Godot;
using Services;
using CharState = CharacterBase.State;

public sealed partial class PlayerAudio : Node {
	private static readonly LogService Log = new(nameof(PlayerAudio), enabled: true);

	public AudioStream? FootstepSound;
	public AudioStream? DamageSound;

	private AudioStreamPlayer? StepPlayer;
	private AudioStreamPlayer? DamagePlayer;
	private Player? OwnerPlayer;
	private double LastFootstepTime;
	private double DamageDuckUntil;

	private const float FootstepVolumeDb = -10.0f;
	private const float FootstepPitchVariance = 0.03f;
	private const float LandVolumeDb = -4.0f;
	private const float LandPitch = 0.82f;
	private const float DamageVolumeDb = -4.0f;
	private const double DamagePitchMin = 0.95;
	private const double DamagePitchMax = 1.03;

	public float WalkStepIntervalSeconds = 0.40f;
	public float SprintStepIntervalSeconds = 0.24f;
	public float DamageDuckSeconds = 0.25f;
	public float DamageFootstepPenaltyDb = -3.0f;

	public void Setup(Player player) {
		OwnerPlayer = player;

		StepPlayer = new AudioStreamPlayer {
			Name = "StepSfx",
			Bus = "SFX",
			VolumeDb = FootstepVolumeDb,
			Stream = FootstepSound,
		};
		AddChild(StepPlayer);

		DamagePlayer = new AudioStreamPlayer {
			Name = "DamageSfx",
			Bus = "SFX",
			VolumeDb = DamageVolumeDb,
			Stream = DamageSound,
		};
		AddChild(DamagePlayer);

		OwnerPlayer.Health.OnChanged += OnHealthChanged;
		Log.Info("Player audio ready");
	}

	public override void _ExitTree() {
		if(OwnerPlayer is Player player) {
			player.Health.OnChanged -= OnHealthChanged;
		}
	}

	private void OnHealthChanged(HealthData from, HealthData to) {
		if(to.Current >= from.Current) { return; }
		if(to.Current == to.Max) { return; }

		DamageDuckUntil = (Time.GetTicksMsec() / 1000.0) + DamageDuckSeconds;

		if(DamagePlayer == null) { return; }

		if(DamagePlayer.Playing) {
			DamagePlayer.Stop();
		}
		DamagePlayer.PitchScale = (float) GD.RandRange(DamagePitchMin, DamagePitchMax);
		DamagePlayer.Play();
	}

	public void ProcessFootsteps(CharState state, double now) {
		float interval = state == CharState.Sprinting ? SprintStepIntervalSeconds : WalkStepIntervalSeconds;

		if((state == CharState.Walking || state == CharState.Sprinting) && now - LastFootstepTime >= interval) {
			PlayFootstep();
		}
	}

	public void PlayFootstep(float pitch = 1.0f, float volumeDb = FootstepVolumeDb) {
		if(StepPlayer == null) { return; }

		double now = Time.GetTicksMsec() / 1000.0;
		float adjustedVolumeDb = now < DamageDuckUntil
			? volumeDb + DamageFootstepPenaltyDb
			: volumeDb;

		StepPlayer.PitchScale = pitch + (float) GD.RandRange(-FootstepPitchVariance, FootstepPitchVariance);
		StepPlayer.VolumeDb = adjustedVolumeDb;
		StepPlayer.Play();
		LastFootstepTime = now;
	}

	public void PlayLand() {
		if(StepPlayer == null) { return; }

		StepPlayer.PitchScale = LandPitch;
		StepPlayer.VolumeDb = LandVolumeDb;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}
}
