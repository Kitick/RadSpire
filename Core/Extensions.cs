using System;
using Godot;

namespace Core {
	public static class Extensions {
		// Vector Components
		public static Vector3 Horizontal(this Vector3 vector) => new Vector3(vector.X, 0, vector.Z);
		public static Vector3 Vertical(this Vector3 vector) => new Vector3(0, vector.Y, 0);

		public static Vector3 ToPolar(float heading, float pitch, float radius = 1) {
			float cosHDG = MathF.Cos(heading);
			float sinHDG = MathF.Sin(heading);
			float cosPIT = MathF.Cos(pitch);
			float sinPIT = MathF.Sin(pitch);

			return new Vector3(
				radius * sinHDG * cosPIT,
				radius * sinPIT,
				radius * cosHDG * cosPIT
			);
		}

		public static float IntersectRay(this Node3D space, Vector3 origin, Vector3 direction, float distance) =>
			space.IntersectRay(origin, origin + direction.Normalized() * distance);

		public static float IntersectRay(this Node3D space, Vector3 origin, Vector3 target) {
			var spaceState = space.GetWorld3D().DirectSpaceState;
			var query = PhysicsRayQueryParameters3D.Create(origin, target);
			query.CollideWithAreas = false;

			var result = spaceState.IntersectRay(query);
			if(result.Count == 0) { return (target - origin).Length(); }

			Vector3 hitpoint = (Vector3) result["position"];
			return origin.DistanceTo(hitpoint);
		}

		// Rotation Smoothing
		public static float SmoothDecay(float speed, float dt) => 1f - MathF.Exp(-speed * dt);

		public static Vector3 SmoothLerp(this Vector3 current, Vector3 target, float speed, float dt) =>
			current.Lerp(target, SmoothDecay(speed, dt));

		public static Quaternion SmoothSlerp(this Quaternion current, Quaternion target, float speed, float dt) =>
			current.Slerp(target, SmoothDecay(speed, dt));

		public static void ApplyRotation(this Node3D node, Vector3 axis, float angle, float speed, float dt) {
			// Get current rotation
			Transform3D currentTransform = node.Transform;
			Quaternion currentRotationQ = new Quaternion(currentTransform.Basis);

			// Calculate target rotation
			Transform3D targetTransform = node.Transform;
			targetTransform.Basis = new Basis(axis, angle);
			Quaternion targetRotationQ = new Quaternion(targetTransform.Basis);

			// Interpolate and apply
			Quaternion newRotationQ = currentRotationQ.SmoothSlerp(targetRotationQ, speed, dt);
			currentTransform.Basis = new Basis(newRotationQ);
			node.Transform = currentTransform;
		}

		// OptionButton
		public static void Populate<T>(this OptionButton button, T[] values) where T : notnull {
			button.Clear();
			foreach(var value in values) {
				button.AddItem(value.ToString());
			}
		}

		public static bool Select<T>(this OptionButton button, T value) where T : notnull {
			string target = value.ToString()!;
			int count = button.GetItemCount();

			for(int i = 0; i < count; i++) {
				if(button.GetItemText(i) == target) {
					button.Select(i);
					return true;
				}
			}
			return false;
		}

		// Node instantiation
		public static T AddScene<T>(this Node node, PackedScene scene) where T : Node {
			var instance = scene.Instantiate<T>();
			node.AddChild(instance);
			return instance;
		}

		public static T AddScene<T>(this Node node, string scene) where T : Node =>
			node.AddScene<T>(GD.Load<PackedScene>(scene));

		public static Node AddScene(this Node node, PackedScene scene) => node.AddScene<Node>(scene);
		public static Node AddScene(this Node node, string scene) => node.AddScene<Node>(scene);
	}
}