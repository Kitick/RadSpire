using Core;
using Godot;
using Services;

namespace Character {
	public sealed partial class PlayerAnimator : AnimationPlayer {
		private static readonly LogService Log = new(nameof(PlayerAnimator), enabled: false);

		private static readonly StringName IDLE = "Idle";
		private static readonly StringName WALKING = "Walking_B";
		private static readonly StringName SPRINTING = "Running_B";
		private static readonly StringName CROUCHING = "Walking_C";
		private static readonly StringName JUMPING = "Jump_Start";
		private static readonly StringName FALLING = "Jump_Idle";
		private static readonly StringName LANDING = "Jump_Land";
		private static readonly StringName DEATH = "Death_A";
		private static readonly StringName SLASH = "1H_Melee_Attack_Slice_Diagonal";

		public enum AnimState { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, Attacking, Dying }

		[Export] private Player Player = null!;

		public AnimState PlayingAnimation {
			get;
			private set {
				field = value;
				switch(value) {
					case AnimState.Idle: Play(IDLE); break;
					case AnimState.Walking: Play(WALKING); break;
					case AnimState.Sprinting: Play(SPRINTING, 0.1f, 1.5f); break;
					case AnimState.Crouching: Play(CROUCHING); break;
					case AnimState.Jumping: Play(JUMPING); break;
					case AnimState.Falling: Play(FALLING); break;
					case AnimState.Landing: Play(LANDING); break;
					case AnimState.Attacking: Play(SLASH); break;				}
			}
		}

		public override void _Ready() {
			this.ValidateExports();

			Player.OnStateChanged += OnPlayerMovement;
			SetupAnimations();
			SyncAnimation(Player.CurrentState);
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
			if(name == JUMPING || name == LANDING) { SyncAnimation(Player.CurrentState); }
			else if(name == SLASH) { Player.OnAttackFinished(); }
		}

		public void SyncAnimation(Player.State state) {
			Log.Info($"Syncing animation to: {state}");

			PlayingAnimation = state switch {
				Player.State.Idle => AnimState.Idle,
				Player.State.Walking => AnimState.Walking,
				Player.State.Sprinting => AnimState.Sprinting,
				Player.State.Crouching => AnimState.Crouching,
				Player.State.Falling => AnimState.Falling,
				Player.State.Attacking => AnimState.Attacking,
				_ => PlayingAnimation,
			};
		}

		private void OnPlayerMovement(Player.State from, Player.State to) {
			Log.Info($"Player State change: {from} -> {to}");

			bool jumped = from != Player.State.Falling && to == Player.State.Falling;
			bool landed = from == Player.State.Falling && to != Player.State.Falling;

			if(jumped) { PlayingAnimation = AnimState.Jumping; }
			else if(landed) { PlayingAnimation = AnimState.Landing; }
			else { SyncAnimation(to); }
		}
	}
}
