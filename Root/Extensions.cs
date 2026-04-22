namespace Root;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Godot;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;

public static class NumericExtensions {
	public static T Round<T>(this T value, T? step = default) where T : INumber<T> {
		double d = double.CreateTruncating(value);
		double s = step is T t ? double.CreateTruncating(t) : 1.0;
		return T.CreateTruncating(Math.Round(d / s) * s);
	}
}

public static class MathExtensions {
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

	public static float IntersectRay(this Node3D space, Vector3 origin, Vector3 direction, float distance) =>
		space.IntersectRay(origin, origin + direction.Normalized() * distance);

	public static float IntersectRay(this Node3D space, Vector3 origin, Vector3 target) {
		return space.IntersectRay(origin, target, null);
	}

	public static float IntersectRay(this Node3D space, Vector3 origin, Vector3 target, IEnumerable<Rid>? exclusions) {
		var spaceState = space.GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(origin, target);
		query.CollideWithAreas = false;
		if(exclusions is not null) {
			var exclude = new Godot.Collections.Array<Rid>();
			foreach(Rid rid in exclusions) {
				if(rid.IsValid) {
					exclude.Add(rid);
				}
			}
			query.Exclude = exclude;
		}

		var result = spaceState.IntersectRay(query);
		if(result.Count == 0) { return (target - origin).Length(); }

		Vector3 hitpoint = (Vector3) result["position"];
		return origin.DistanceTo(hitpoint);
	}
}

public static class NodeExtensions {
	public static void ValidateExports(this Node node) {
		Type type = node.GetType();
		foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
			if(!field.IsDefined(typeof(ExportAttribute), inherit: false)) { continue; }
			if(field.GetValue(node) is not null) { continue; }
			GD.PushError($"[{type.Name}] {field.Name} is missing export assignment!");
		}
	}

	// Node instantiation
	public static Node AddScene(this Node node, PackedScene scene) => node.AddScene<Node>(scene);
	public static TScene AddScene<TScene>(this Node node, PackedScene scene) where TScene : Node {
		TScene instance = scene.Instantiate<TScene>();
		node.AddChild(instance);
		return instance;
	}

	// OptionButton
	public static void Populate<T>(this OptionButton button, params IEnumerable<T> values) where T : notnull {
		button.Clear();
		foreach(var value in values) {
			button.AddItem(value.ToString());
		}
	}

	public static bool SelectItem<T>(this OptionButton button, T value) where T : notnull {
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

	public static T? GetSelectedItem<T>(this OptionButton button, params IEnumerable<T> values) where T : class {
		int index = button.Selected;
		if(index < 0 || index >= values.Count()) { return null; }
		return values.ElementAt(index);
	}
}
