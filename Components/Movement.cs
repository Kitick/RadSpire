using System;
using Core;
using Godot;
using SaveSystem;

namespace Components {
	public class Movement : ISaveable<MovementData> {
		public float BaseSpeed = 3.0f;
		public float RotationSpeed = 2.0f;
		public float JumpSpeed = 4.5f;
		public float Friction = 10.0f;

		private readonly CharacterBody3D Body;

		public Movement(CharacterBody3D body) => Body = body;

		public void Update(float dt) {
			if(Body.IsOnFloor()) { ApplyFriction(dt); }
			else { Fall(dt); }

			UpdateRotation(dt);
			Body.MoveAndSlide();
		}

		public void Move(Vector3 direction, float multiplier) {
			Vector3 move = direction * BaseSpeed * multiplier;
			Body.Velocity = new Vector3(move.X, Body.Velocity.Y, move.Z);
		}

		public void Jump() {
			Body.Velocity += JumpSpeed * Vector3.Up;
		}

		private void Fall(float dt) {
			Body.Velocity += Numbers.GRAVITY * Vector3.Down * dt;
		}

		private void UpdateRotation(float dt) {
			Vector3 direction = Body.Velocity;

			if(direction.Length() < Numbers.EPSILON) { return; }

			float newRotationAngle = Mathf.RadToDeg(Mathf.Atan2(direction.X, direction.Z));

			Body.ApplyRotation(Vector3.Up, Mathf.DegToRad(newRotationAngle), RotationSpeed, dt);
		}

		private void ApplyFriction(float dt) {
			Vector3 horizontal = Body.Velocity.Horizontal().SmoothLerp(Vector3.Zero, Friction, dt);
			Body.Velocity = horizontal + Body.Velocity.Vertical();
		}

		public MovementData Serialize() => new MovementData {
			Position = Body.GlobalPosition,
			Velocity = Body.Velocity,
			Rotation = Body.RotationDegrees,
		};

		public void Deserialize(in MovementData data) {
			Body.GlobalPosition = data.Position;
			Body.RotationDegrees = data.Rotation;
			Body.Velocity = data.Velocity;
		}
	}
}

namespace SaveSystem {
	public readonly struct MovementData : ISaveData {
		public Vector3 Position { get; init; }
		public Vector3 Velocity { get; init; }
		public Vector3 Rotation { get; init; }
	}
}