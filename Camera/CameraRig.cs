using System;
using Camera;
using Core;
using Godot;
using SaveSystem;

namespace Camera {
	public record struct CameraPose {
		public Vector3 Ground;

		public readonly Vector3 Anchor => Ground + new Vector3(0, Height, 0);
		public readonly float MinPitch => Mathf.RadToDeg(MathF.Asin(-Height / Distance));

		private const float MinDistance = 3f;
		private const float MaxDistance = 20f;
		private const float Height = 1.5f;

		public float Distance { get;
			set {
				field = Math.Clamp(value, MinDistance, MaxDistance);
				Pitch = Pitch; // Re-clamp pitch based on new distance
			}
		}
		public float Heading { get; set => field = (value + 360) % 360; }
		public float Pitch { get; set => field = Math.Clamp(value, MinPitch, 89); }

		public readonly float RadHDG => Mathf.DegToRad(Heading);
		public readonly float RadPIT => Mathf.DegToRad(Pitch);

		public readonly Vector3 CalcPosition() {
			float hdg = Mathf.DegToRad(Heading);
			float pit = Mathf.DegToRad(Pitch);

			float cosHDG = MathF.Cos(hdg);
			float sinHDG = MathF.Sin(hdg);
			float cosPIT = MathF.Cos(pit);
			float sinPIT = MathF.Sin(pit);

			Vector3 orbit = new Vector3(
				Distance * sinHDG * cosPIT,
				Distance * sinPIT,
				Distance * cosHDG * cosPIT
			);

			return Anchor + orbit;
		}
	}

	public sealed partial class CameraRig : Node3D, ISaveable<CameraRigData> {
		public enum CameraState { Idle, Following };
		public CameraState State { get; private set; } = CameraState.Following;

		public CameraPose Pose = new CameraPose() {
			Distance = 10f,
			Heading = 0f,
			Pitch = 45f,
		};

		public Node3D? Target;
		private Camera3D camera = null!;

		private readonly CameraDrag Drag = new CameraDrag();

		public float FollowSpeed = 5.0f;

		private const string Camera3D = "Camera3D";

		public override void _Ready() {
			Drag.ResetTimer.Timeout += Reset;
			AddChild(Drag.ResetTimer);

			camera = GetNode<Camera3D>(Camera3D);
		}

		public override void _PhysicsProcess(double delta) {
			float dt = (float) delta;

			if(State == CameraState.Following) {
				FollowTarget(dt);
			}
			else if(State == CameraState.Idle && TargetHasMoved()) {
				Reset();
			}

			Drag.Update(Pose.Ground, dt);

			Pose.Ground += Drag.Velocity * dt;

			GlobalPosition = Pose.CalcPosition();

			LookAt(Pose.Anchor, Vector3.Up);
		}

		private void Reset() {
			State = CameraState.Following;
			Drag.Reset();
		}

		private void FollowTarget(float dt) {
			if(Target == null) { return; }
			Pose.Ground = Pose.Ground.SmoothLerp(Target.GlobalPosition, FollowSpeed, dt);
		}

		private bool TargetHasMoved() {
			if(Target is not CharacterBody3D body) { return false; }

			float targetSpeed = body.Velocity.Length();

			bool moved = targetSpeed > Numbers.EPSILON;

			return moved;
		}

		public CameraRigData Serialize() => new CameraRigData {
			Pose = Pose,
		};

		public void Deserialize(in CameraRigData data) {
			Pose = data.Pose;
		}
	}
}

namespace SaveSystem {
	public readonly record struct CameraRigData : ISaveData {
		public CameraPose Pose { get; init; }
	}
}