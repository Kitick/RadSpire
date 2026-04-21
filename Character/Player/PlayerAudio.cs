namespace Character;

using Godot;
using Services;
using CharState = CharacterBase.State;

public sealed partial class PlayerAudio : Node {
	private static readonly LogService Log = new(nameof(PlayerAudio), enabled: true);
	private const string StepSoundPath = "res://Assets/Audio/step.wav";

	private AudioStreamPlayer? StepPlayer;
	private AudioStream? StepSound;
	private double LastFootstepTime;

	public float WalkStepIntervalSeconds = 0.40f;
	public float SprintStepIntervalSeconds = 0.24f;

	public void Setup() {
		StepSound = GD.Load<AudioStream>(StepSoundPath);

		if(StepSound == null) {
			Log.Warn($"Step sound not found at {StepSoundPath}");
			return;
		}

		StepPlayer = new AudioStreamPlayer {
			Name = "StepSfx",
			Bus = "SFX",
			VolumeDb = -5.0f,
			Stream = StepSound,
		};

		AddChild(StepPlayer);
		Log.Info("Step audio ready");
	}

	public void ProcessFootsteps(CharState state, double now) {
		float interval = state == CharState.Sprinting ? SprintStepIntervalSeconds : WalkStepIntervalSeconds;

		if((state == CharState.Walking || state == CharState.Sprinting) && now - LastFootstepTime >= interval) {
			PlayFootstep();
		}
	}

	public void PlayFootstep(float pitch = 1.0f, float volumeDb = -5.0f) {
		if(StepPlayer == null || StepSound == null) { return; }

		StepPlayer.PitchScale = pitch + (float) GD.RandRange(-0.03, 0.03);
		StepPlayer.VolumeDb = volumeDb;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}

	public void PlayLand() {
		if(StepPlayer == null || StepSound == null) { return; }

		StepPlayer.PitchScale = 0.82f;
		StepPlayer.VolumeDb = -1.5f;
		StepPlayer.Play();
		LastFootstepTime = Time.GetTicksMsec() / 1000.0;
	}
}