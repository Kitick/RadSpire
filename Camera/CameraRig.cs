using System;
using Camera;
using Core;
using Godot;
using SaveSystem;

namespace Camera {
	public record struct CameraPose {
		public Vector3 Ground;

		public float Distance { get; set => field = Math.Max(value, 0); }
		public float Heading { get; set => field = value % 360; }
		public float Pitch { get; set => field = Math.Clamp(value, -90, 90); }

		public readonly Vector3 CalcPosition() {
			float cosHDG = MathF.Cos(Heading);
			float sinHDG = MathF.Sin(Heading);
			float cosPIT = MathF.Cos(Pitch);
			float sinPIT = MathF.Sin(Pitch);

			Vector3 orbit = new Vector3(
				Distance * sinHDG * cosPIT,
				Distance * sinPIT,
				Distance * cosHDG * cosPIT
			);

			return Ground + orbit;
		}
	}

	public partial class CameraRig : Node3D, ISaveable<CameraRigData> {
		public enum CameraState { Idle, Following, Panning };
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

			LookAt(Pose.Ground, Vector3.Up);
		}

		private void Reset() {
			State = CameraState.Following;
			Drag.Reset();
		}

		private void FollowTarget(float dt) {
			if(Target == null){ return; }
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