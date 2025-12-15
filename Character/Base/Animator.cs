using System;
using Godot;
using Services;

using CharState = Character.CharacterBase.State;

namespace Character {
	public sealed partial class Animator : AnimationPlayer {
		private static readonly LogService Log = new(nameof(Animator), enabled: true);

		[Export] private StringName IDLE = null!;
		[Export] private StringName WALKING = null!;
		[Export] private StringName SPRINTING = null!;
		[Export] private StringName CROUCHING = null!;
		[Export] private StringName JUMPING = null!;
		[Export] private StringName FALLING = null!;
		[Export] private StringName LANDING = null!;
		[Export] private StringName DEATH = null!;
		[Export] private StringName ATTACK = null!;

		public enum AnimState { Idle, Walking, Sprinting, Crouching, Jumping, Falling, Landing, Attacking, Dying }

		[Export] private CharacterBase Character = null!;

		private AnimState PlayingAnimation {
			get;
			set {
				field = value;
				switch(value) {
					case AnimState.Idle: Play(IDLE); break;
					case AnimState.Walking: Play(WALKING); break;
					case AnimState.Sprinting: Play(SPRINTING); break;
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
			Character.OnStateChanged += OnMovement;
			SetupAnimations();
			SyncAnimation(Character.CurrentState);
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
			if(name == JUMPING || name == LANDING) {
				SyncAnimation(Character.CurrentState);
			}
		}

		public void SyncAnimation(CharState state) {
			Log.Info($"Syncing animation to: {state}");

			PlayingAnimation = state switch {
				CharState.Idle => AnimState.Idle,
				CharState.Walking => AnimState.Walking,
				CharState.Sprinting => AnimState.Sprinting,
				CharState.Crouching => AnimState.Crouching,
				CharState.Falling => AnimState.Falling,
				_ => PlayingAnimation,
			};
		}

		private void OnMovement(CharState from, CharState to) {
			Log.Info($"Player State change: {from} -> {to}");

			bool jumped = from != CharState.Falling && to == CharState.Falling;
			bool landed = from == CharState.Falling && to != CharState.Falling;

			if(jumped) { PlayingAnimation = AnimState.Jumping; }
			else if(landed) { PlayingAnimation = AnimState.Landing; }
			else { SyncAnimation(to); }
		}
	}
}
