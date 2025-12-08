using Godot;
using Services;

namespace Character {
	public sealed partial class PlayerAnimator : AnimationPlayer {
		private static readonly LogService Log = new(nameof(PlayerAnimator), enabled: true);

		public enum AnimState { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, ATTACKING, DYING }

		[Export] private Player Player = null!;

		private const string IDLE = "Idle";
		private const string WALKING = "Walking_B";
		private const string SPRINTING = "Running_B";
		private const string CROUCHING = "Walking_C";
		private const string JUMPING = "Jump_Start";
		private const string FALLING = "Jump_Idle";
		private const string LANDING = "Jump_Land";
		private const string DEATH = "Death_A";
		private const string SLASH = "1H_Melee_Attack_Slice_Diagonal";

		public AnimState PlayingAnimation {
			get;
			private set {
				field = value;
				switch(value) {
					case AnimState.Idle: Play(IDLE); break;
					case AnimState.Walking: Play(WALKING); break;
					case AnimState.Sprinting: Play(SPRINTING); break;
					case AnimState.Crouching: Play(CROUCHING); break;
					case AnimState.Jumping: Play(JUMPING); break;
					case AnimState.Falling: Play(FALLING); break;
					case AnimState.Landing: Play(LANDING); break;
					case AnimState.ATTACKING: Play(SLASH); break;
					case AnimState.DYING: Play(DEATH); break;
				}
			}
		}

		public override void _Ready() {
			// sync state machine

			SetupAnimations();
		}

		private void SetupAnimations() {
			SetLoopMode(IDLE);
			SetLoopMode(WALKING);
			SetLoopMode(SPRINTING);
			SetLoopMode(CROUCHING);
			SetLoopMode(FALLING);

			AnimationFinished += OnAnimationFinished;
			SyncAnimation();
		}

		private void SetLoopMode(string name) {
			GetAnimation(name).LoopMode = Animation.LoopModeEnum.Linear;
		}

		public void OnAnimationFinished(StringName name) {
			if(name == JUMPING || name == LANDING) {
				SyncAnimation();
			}
		}

		public void SyncAnimation() {
			Log.Info($"Syncing animation to: {Player.CurrentState}");

			PlayingAnimation = Player.CurrentState switch {
				Player.State.Idle => AnimState.Idle,
				Player.State.Walking => AnimState.Walking,
				Player.State.Sprinting => AnimState.Sprinting,
				Player.State.Crouching => AnimState.Crouching,
				Player.State.Falling => AnimState.Falling,
				_ => PlayingAnimation,
			};
		}

		private void OnPlayerMovement(Player.State from, Player.State to) {
			Log.Info($"Player State change: {from} -> {to}");

			bool jumped = from != Player.State.Falling && to == Player.State.Falling;
			bool landed = from == Player.State.Falling && to != Player.State.Falling;

			if(jumped) { PlayingAnimation = AnimState.Jumping; }
			else if(landed) { PlayingAnimation = AnimState.Landing; }
			else { SyncAnimation(); }
		}
	}
}