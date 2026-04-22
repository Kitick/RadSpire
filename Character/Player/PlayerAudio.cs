namespace Character;

using Components;
using Godot;
using Services;
using CharState = CharacterBase.State;

public sealed partial class PlayerAudio : Node {
	private static readonly LogService Log = new(nameof(PlayerAudio), enabled: true);
	private const string StepSoundPath = "res://Assets/Audio/NewStep.wav";
	private const string DamageSoundPath = "res://Assets/Audio/PlayerTakeDamage.wav";

	private AudioStreamPlayer? StepPlayer;
	private AudioStreamPlayer? DamagePlayer;
	private AudioStream? StepSound;
	private AudioStream? DamageSound;
	private Player? OwnerPlayer;
	private double LastFootstepTime;
	private double DamageDuckUntil;

	public float WalkStepIntervalSeconds = 0.40f;
	public float SprintStepIntervalSeconds = 0.24f;
	public float DamageDuckSeconds = 0.25f;
	public float DamageFootstepPenaltyDb = -3.0f;

	public void Setup(Player player) {
		OwnerPlayer = player;
		StepSound = GD.Load<AudioStream>(StepSoundPath);
		DamageSound = GD.Load<AudioStream>(DamageSoundPath);

		if(StepSound == null) {
			Log.Warn($"Step sound not found at {StepSoundPath}");
		} 
        else {
			StepPlayer = new AudioStreamPlayer {
				Name = "StepSfx",
				Bus = "SFX",
				VolumeDb = -12.0f,
				Stream = StepSound,
			};
			AddChild(StepPlayer);
		}

		if(DamageSound == null) {
			Log.Warn($"Damage sound not found at {DamageSoundPath}");
		} 
        else {
			DamagePlayer = new AudioStreamPlayer {
				Name = "DamageSfx",
				Bus = "SFX",
				VolumeDb = -4.0f,
				Stream = DamageSound,
			};
			AddChild(DamagePlayer);
		}

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

		DamageDuckUntil = Time.GetTicksMsec() / 1000.0 + DamageDuckSeconds;

		if(DamagePlayer == null || DamageSound == null) {
			Log.Warn("Damage audio skipped: DamagePlayer or DamageSound is null");
			return;
		}

		DamagePlayer.Stream = DamageSound;
		if(DamagePlayer.Playing) {
			DamagePlayer.Stop();
		}
		DamagePlayer.PitchScale = (float) GD.RandRange(0.95, 1.03);
		DamagePlayer.Play();
	}

	public void ProcessFootsteps(CharState state, double now) {
		float interval = state == CharState.Sprinting ? SprintStepIntervalSeconds : WalkStepIntervalSeconds;

		if((state == CharState.Walking || state == CharState.Sprinting) && now - LastFootstepTime >= interval) {
			PlayFootstep();
		}
	}

	public void PlayFootstep(float pitch = 1.0f, float volumeDb = -12.0f) {
		if(StepPlayer == null || StepSound == null) { return; }

		double now = Time.GetTicksMsec() / 1000.0;
		float adjustedVolumeDb = now < DamageDuckUntil
			? volumeDb + DamageFootstepPenaltyDb
			: volumeDb;

		StepPlayer.PitchScale = pitch + (float) GD.RandRange(-0.03, 0.03);
		StepPlayer.VolumeDb = adjustedVolumeDb;
		StepPlayer.Play();
		LastFootstepTime = now;
	}

	public void PlayLand() {
		if(StepPlayer == null || StepSound == null) { return; }

		StepPlayer.PitchScale = 0.82f;
		StepPlayer.VolumeDb = -8.5f;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}
}