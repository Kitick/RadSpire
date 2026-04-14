namespace Camera;

using System;
using Godot;
using Root;
using Services;

public sealed partial class CameraRig : Node3D, ISaveable<CameraRigData> {
	public enum CameraState { Idle, Following };
	public CameraState State { get; private set; } = CameraState.Following;

	public CameraPose Pose = new CameraPose() {
		Distance = 10f,
		Heading = 0f,
		Pitch = 45f,
	};

	[Export] private Camera3D Camera = null!;
	[Export] private ShapeCast3D CameraShapeCast = null!;
	[Export] private Vector3 TargetOffset = new Vector3(0, 1.5f, 0);
	private readonly CameraDrag Drag = new();

	public Node3D? Target;
	public CollidingObjects CollidingObjects { get; } = new CollidingObjects();

	public override void _Ready() {
		Camera ??= GetNodeOrNull<Camera3D>("Camera3D") ?? GetNodeOrNull<Camera3D>("Camera");
		if(!IsInstanceValid(Camera)) {
			GD.PushWarning("CameraRig: Camera3D export is not assigned and no child camera was found.");
		}

		CameraShapeCast ??= Camera?.GetNodeOrNull<ShapeCast3D>("ShapeCast3D");
		CameraShapeCast ??= GetNodeOrNull<ShapeCast3D>("ShapeCast3D");

		Drag.ResetTimer.Timeout += Reset;
		AddChild(Drag.ResetTimer);

		InitInput();
	}

	public override void _ExitTree() {
		RestoreAllFadedWalls();
		Unsubscribe?.Invoke();
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float) delta;

		HandleJoystickRotation(dt);

		if(State == CameraState.Following) {
			FollowTarget(dt);
		}
		else if(State == CameraState.Idle && TargetHasMoved()) {
			Reset();
		}

		Drag.Update(Pose.Ground);

		Pose.Ground += Drag.Velocity * dt;

		GlobalPosition = Pose.CalcPosition(this);

		LookAt(Pose.Anchor, Vector3.Up);
		UpdateShapeCastTarget();
		UpdateCollidingWalls();
	}

	private void UpdateShapeCastTarget() {
		if(!IsInstanceValid(CameraShapeCast)) { return; }

		Vector3 targetGlobalPosition = GlobalPosition;
		if(IsInstanceValid(Target)) {
			targetGlobalPosition = Target.GlobalPosition + TargetOffset;
		}

		CameraShapeCast.TargetPosition = CameraShapeCast.ToLocal(targetGlobalPosition);
	}

	private void UpdateCollidingWalls() {
		if(!IsInstanceValid(CameraShapeCast)) { return; }
		try {
			CameraShapeCast.ForceShapecastUpdate();
			CollidingObjects.BeginFrame();

			int collisionCount = CameraShapeCast.GetCollisionCount();
			for(int i = 0; i < collisionCount; i++) {
				Node? colliderNode = CameraShapeCast.GetCollider(i) as Node;
				Node3D? wall = FindFadeWallRoot(colliderNode);
				if(!IsInstanceValid(wall)) { continue; }

				CollidingObjects.AddCurrentWall(wall);
			}

			CollidingObjects.EndFrame();
		}
		catch(Exception ex) {
			GD.PushWarning($"Wall fading failed: {ex.Message}");
			CollidingObjects.Clear();
		}
	}

	private static Node3D? FindFadeWallRoot(Node? node) {
		Node? current = node;
		while(current is not null) {
			if(current is Node3D wallNode && wallNode is ICameraFadingObject) {
				return wallNode;
			}

			current = current.GetParent();
		}

		return null;
	}

	private void RestoreAllFadedWalls() {
		CollidingObjects.Clear();
	}

	private void Reset() {
		State = CameraState.Following;
		Drag.Reset();
	}

	private void FollowTarget(float dt) {
		if(!IsInstanceValid(Target)) { return; }
		Pose.Ground = Pose.Ground.SmoothLerp(Target.GlobalPosition, FollowSpeed, dt);
	}

	private bool TargetHasMoved() {
		if(!IsInstanceValid(Target)) { return false; }
		if(Target is not CharacterBody3D body) { return false; }

		float targetSpeed = body.Velocity.Length();

		bool moved = targetSpeed > Numbers.EPSILON;

		return moved;
	}

	public CameraRigData Export() => new CameraRigData {
		Pose = Pose,
	};

	public void Import(CameraRigData data) {
		Pose = data.Pose;
	}
}

public record struct CameraPose {
	public Vector3 Ground;
	public readonly Vector3 Anchor => Ground + new Vector3(0, Height, 0);

	public const float Height = 1.5f;
	public const float MinDistance = 1.5f;
	public const float MaxDistance = 20f;
	public const float BufferDistance = 0.25f;

	public float Distance { get; set => field = Math.Clamp(value, MinDistance, MaxDistance); }
	public float Heading { get; set => field = (value + 360) % 360; }
	public float Pitch { get; set => field = Math.Clamp(value, -89, 89); }

	private readonly float RadHDG => Mathf.DegToRad(Heading);
	private readonly float RadPIT => Mathf.DegToRad(Pitch);

	public readonly Vector2 AlignVector(Vector2 direction) => direction.Rotated(-RadHDG);
	public readonly Vector3 AlignVector(Vector3 direction) => direction.Rotated(Vector3.Up, RadHDG);

	public readonly Vector3 CalcPosition(Node3D space) {
		Vector3 direction = MathExtensions.ToPolar(RadHDG, RadPIT);

		float distance = Math.Min(
			Distance,
			space.IntersectRay(
				Anchor + direction * MinDistance,
				Anchor + direction * MaxDistance,
				CameraCollisionExclusions.GetAll()
			) - BufferDistance
		);
		distance = Math.Max(distance, MinDistance);

		Vector3 orbit = direction * distance;

		return Anchor + orbit;
	}
}

public readonly record struct CameraRigData : ISaveData {
	public CameraPose Pose { get; init; }
}
