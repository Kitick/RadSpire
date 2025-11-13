using System;
using Godot;

public partial class PlayerAnimator : Node3D {
	private const string IDLE = "Idle";
	private const string WALKING = "Walking_B";
	private const string RUNNING = "Running_B";
	private const string CROUCHING = "Walking_C";
	private const string JUMP_START = "Jump_Start";
	private const string JUMP_IDLE = "Jump_Idle";
	private const string JUMP_LAND = "Jump_Land";

	private Player Player = null!;
	private AnimationPlayer AnimationPlayer = null!;

	private Player.MovementEvent currentAction = Player.MovementEvent.Stop;

	public override void _Ready() {
		GetComponents();
		SetupAnimations();
	}

	private void GetComponents() {
		Player = GetParent<Player>();
		Player.PlayerMovement += OnPlayerMovement;
		OnPlayerMovement(Player.MovementEvent.Stop);

		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void SetupAnimations() {
		SetLoopMode(IDLE);
		SetLoopMode(WALKING);
		SetLoopMode(RUNNING);
		SetLoopMode(CROUCHING);
		SetLoopMode(JUMP_IDLE);

		AnimationPlayer.Play(IDLE);
		currentAction = Player.MovementEvent.Stop;
	}

	private void SetLoopMode(string name) {
		AnimationPlayer.GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
	}

	void OnPlayerMovement(Player.MovementEvent action) {
		if(currentAction == action) { return; }
		currentAction = action;

		switch(currentAction) {
			case Player.MovementEvent.Start:
				AnimationPlayer.Play(WALKING);
				break;
			case Player.MovementEvent.Stop:
				AnimationPlayer.Play(IDLE);
				break;
			case Player.MovementEvent.Jump:
				AnimationPlayer.Play(JUMP_START);
				AnimationPlayer.Queue(JUMP_IDLE);
				break;
			case Player.MovementEvent.Land:
				AnimationPlayer.Play(JUMP_LAND);
				break;
			case Player.MovementEvent.SprintStart:
				AnimationPlayer.Play(RUNNING);
				break;
			case Player.MovementEvent.SprintStop:
				AnimationPlayer.Play(WALKING);
				break;
			case Player.MovementEvent.CrouchStart:
				AnimationPlayer.Play(CROUCHING);
				break;
			case Player.MovementEvent.CrouchStop:
				AnimationPlayer.Play(WALKING);
				break;
		}
	}
}