using System;
using Godot;

public partial class PlayerAnimator : Node3D {
	[Export] private Player? player;
	[Export] private AnimationPlayer? animationPlayer;
	private string currentAction;

	public override void _Ready() {
		player = GetParent<Player>();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		if(player != null) {
			player.PlayerMovement += OnPlayerMovement;
			OnPlayerMovement("move_stop");
		}
		if(animationPlayer != null) {
			Animation? a;
			a = animationPlayer.GetAnimation("Idle");
			if(a != null) {
				a.LoopMode = Animation.LoopModeEnum.Linear;
			}
			a = animationPlayer.GetAnimation("Walking_B");
			if(a != null) {
				a.LoopMode = Animation.LoopModeEnum.Linear;
			}
			a = animationPlayer.GetAnimation("Running_B");
			if(a != null) {
				a.LoopMode = Animation.LoopModeEnum.Linear;
			}
			a = animationPlayer.GetAnimation("Walking_C");
			if(a != null) {
				a.LoopMode = Animation.LoopModeEnum.Linear;
			}
			a = animationPlayer.GetAnimation("Jump_Idle");
			if(a != null) {
				a.LoopMode = Animation.LoopModeEnum.Linear;
			}
			animationPlayer.Play("Idle");
			currentAction = "move_stop";
		}
	}

	void OnPlayerMovement(string action) {
		if(animationPlayer == null || player == null) {
			return;
		}
		if(currentAction == action) {
			return;
		}
		currentAction = action;
		switch(currentAction) {
			case "move_start":
				animationPlayer.Play("Walking_B");
				break;
			case "move_stop":
				animationPlayer.Play("Idle");
				break;
			case "jump":
				animationPlayer.Play("Jump_Start");
				animationPlayer.Queue("Jump_Idle");
				break;
			case "land":
				animationPlayer.Play("Jump_Land");
				break;
			case "sprint_start":
				animationPlayer.Play("Running_B");
				break;
			case "sprint_stop":
				animationPlayer.Play("Walking_B");
				break;
			case "crouch_start":
				animationPlayer.Play("Walking_C");
				break;
			case "crouch_stop":
				animationPlayer.Play("Walking_B");
				break;
		}
	}
}