using System;
using Godot;

namespace Components {
	public class Movement {
		public float BaseSpeed = 2.0f;
		public float RotationSpeed = 2.0f;
		public float JumpSpeed = 4.5f;
		public float Friction = 10.0f;

		private const float GRAVITY = 9.8f;
		private const float EPSILON = 0.01f;

		private readonly CharacterBody3D Body;

		public Movement(CharacterBody3D body) => Body = body;

		public void Update(float dt) {
			if(Body.IsOnFloor()) { ApplyFriction(dt); }
			else { Fall(dt); }

			UpdateRotation(dt);
			Body.MoveAndSlide();
		}

		public void Move(Vector3 direction, float multiplier) {
			var move = direction * BaseSpeed * multiplier;
			Body.Velocity = new Vector3(move.X, Body.Velocity.Y, move.Z);
		}

		public void Jump() {
			Body.Velocity += JumpSpeed * Vector3.Up;
		}

		private void Fall(float dt) {
			Body.Velocity += GRAVITY * Vector3.Down * dt;
		}

		private void UpdateRotation(float dt) {
			Vector3 direction = Body.Velocity;

			if(direction.Length() < EPSILON) { return; }

			// Calculate target rotation
			float newRotationAngle = Mathf.RadToDeg(Mathf.Atan2(direction.X, direction.Z));
			Transform3D newRotation = Body.Transform;
			newRotation.Basis = new Basis(Vector3.Up, Mathf.DegToRad(newRotationAngle));

			// Get current rotation
			Transform3D curRotation = Body.Transform;
			Quaternion curRotationQ = new Quaternion(curRotation.Basis);

			// Slerp towards target rotation
			Quaternion newRotationQ = new Quaternion(newRotation.Basis);
			float rotationSpeed = RotationSpeed * direction.Length();
			curRotationQ = curRotationQ.Slerp(newRotationQ, rotationSpeed * dt);

			// Apply rotation
			curRotation.Basis = new Basis(curRotationQ);
			Body.Transform = curRotation;
		}

		private void ApplyFriction(float dt) {
			float weight = 1f - Mathf.Exp(-Friction * dt);
			var x = Mathf.Lerp(Body.Velocity.X, 0.0f, weight);
			var z = Mathf.Lerp(Body.Velocity.Z, 0.0f, weight);
			Body.Velocity = new Vector3(x, Body.Velocity.Y, z);
		}
	}
}