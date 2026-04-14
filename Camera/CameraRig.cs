namespace Camera;

using System;
using System.Collections.Generic;
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
	[Export(PropertyHint.Range, "1,30,0.5")] private float CollisionZoomInSpeed = 18f;
	[Export(PropertyHint.Range, "1,30,0.5")] private float CollisionZoomOutSpeed = 8f;
	[Export(PropertyHint.Range, "1,10,1")] private int WallFadeDebounceFrames = 4;
	[Export(PropertyHint.Range, "0,2,0.05")] private float BackfaceProbeRadius = 0.45f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BackfaceDotThreshold = 0.05f;
	private readonly CameraDrag Drag = new();
	private float CurrentCollisionDistance;
	private bool HasCollisionDistance;

	public Node3D? Target;
	public CollidingObjects CollidingObjects { get; } = new CollidingObjects();

	public override void _Ready() {
		Camera ??= GetNodeOrNull<Camera3D>("Camera3D") ?? GetNodeOrNull<Camera3D>("Camera");
		if(!IsInstanceValid(Camera)) {
			GD.PushWarning("CameraRig: Camera3D export is not assigned and no child camera was found.");
		}

		if(CameraShapeCast is null && IsInstanceValid(Camera)) {
			CameraShapeCast = Camera.GetNodeOrNull<ShapeCast3D>("ShapeCast3D");
		}

		if(CameraShapeCast is null) {
			CameraShapeCast = GetNodeOrNull<ShapeCast3D>("ShapeCast3D");
		}

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

		UpdateCameraPosition(dt);

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

	private void UpdateCameraPosition(float dt) {
		Vector3 direction = MathExtensions.ToPolar(Mathf.DegToRad(Pose.Heading), Mathf.DegToRad(Pose.Pitch));
		float targetDistance = CalculateCollisionLimitedDistance(direction);
		if(!HasCollisionDistance) {
			CurrentCollisionDistance = targetDistance;
			HasCollisionDistance = true;
		}

		float smoothingSpeed = targetDistance < CurrentCollisionDistance ? CollisionZoomInSpeed : CollisionZoomOutSpeed;
		CurrentCollisionDistance = Mathf.Lerp(CurrentCollisionDistance, targetDistance, MathExtensions.SmoothDecay(smoothingSpeed, dt));
		GlobalPosition = Pose.Anchor + direction * CurrentCollisionDistance;
	}

	private float CalculateCollisionLimitedDistance(Vector3 direction) {
		float rayDistance = this.IntersectRay(
			Pose.Anchor + direction * CameraPose.MinDistance,
			Pose.Anchor + direction * CameraPose.MaxDistance,
			CameraCollisionExclusions.GetAll()
		) - CameraPose.BufferDistance;

		float unclampedDistance = Math.Min(Pose.Distance, rayDistance);
		return Math.Clamp(unclampedDistance, CameraPose.MinDistance, Pose.Distance);
	}

	private void UpdateCollidingWalls() {
		if(!IsInstanceValid(CameraShapeCast)) { return; }
		CollidingObjects.FadeDebounceFrames = WallFadeDebounceFrames;
		try {
			CameraShapeCast.ForceShapecastUpdate();
			CollidingObjects.BeginFrame();
			PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

			int collisionCount = CameraShapeCast.GetCollisionCount();
			for(int i = 0; i < collisionCount; i++) {
				Node? colliderNode = CameraShapeCast.GetCollider(i) as Node;
				Node3D? wall = FindFadeWallRoot(colliderNode);
				if(!IsInstanceValid(wall)) { continue; }

				CollidingObjects.AddCurrentWall(wall);
			}
			Node3D? desiredZoomBlockingWall = FindDesiredZoomBlockingWall();
			if(IsInstanceValid(desiredZoomBlockingWall)) {
				CollidingObjects.AddCurrentWall(desiredZoomBlockingWall);
			}
			AddBackfaceVisibleWalls(spaceState);

			CollidingObjects.EndFrame();
		}
		catch(Exception ex) {
			GD.PushWarning($"Wall fading failed: {ex.Message}");
			CollidingObjects.Clear();
		}
	}

	private void AddBackfaceVisibleWalls(PhysicsDirectSpaceState3D spaceState) {
		if(BackfaceProbeRadius <= 0f) {
			return;
		}

		Vector3 cameraPosition = GlobalPosition;
		Vector3 anchor = Pose.Anchor;
		Vector3 horizontalToCamera = (cameraPosition - anchor).Horizontal();
		if(horizontalToCamera.LengthSquared() < 0.0001f) {
			return;
		}

		Vector3 forward = horizontalToCamera.Normalized();
		Vector3 right = forward.Cross(Vector3.Up).Normalized();
		Vector3[] probePoints = [
			anchor,
			anchor + right * BackfaceProbeRadius,
			anchor - right * BackfaceProbeRadius,
			anchor + forward * BackfaceProbeRadius,
			anchor - forward * BackfaceProbeRadius,
		];

		foreach(Vector3 probePoint in probePoints) {
			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(cameraPosition, probePoint);
			query.CollideWithAreas = false;

			if(CameraCollisionExclusions.GetAll().Count > 0) {
				Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
				foreach(Rid rid in CameraCollisionExclusions.GetAll()) {
					if(rid.IsValid) {
						exclude.Add(rid);
					}
				}
				query.Exclude = exclude;
			}

			Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
			if(result.Count == 0) {
				continue;
			}

			Node? colliderNode = result.ContainsKey("collider") ? result["collider"].AsGodotObject() as Node : null;
			Node3D? wall = FindFadeWallRoot(colliderNode);
			if(!IsInstanceValid(wall) || !result.ContainsKey("normal")) {
				continue;
			}

			Vector3 hitNormal = (Vector3) result["normal"];
			Vector3 rayDirection = (probePoint - cameraPosition).Normalized();
			bool isBackfaceHit = hitNormal.Dot(rayDirection) > BackfaceDotThreshold;
			if(isBackfaceHit) {
				CollidingObjects.AddCurrentWall(wall);
			}
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

	private Node3D? FindDesiredZoomBlockingWall() {
		if(GetWorld3D() is null) { return null; }
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		Vector3 desiredPosition = Pose.CalcDesiredPosition();
		HashSet<Rid> exclusions = new(CameraCollisionExclusions.GetAll());

		for(int i = 0; i < 8; i++) {
			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(Pose.Anchor, desiredPosition);
			query.CollideWithAreas = false;
			if(exclusions.Count > 0) {
				Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
				foreach(Rid rid in exclusions) {
					if(rid.IsValid) {
						exclude.Add(rid);
					}
				}
				query.Exclude = exclude;
			}

			Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
			if(result.Count == 0) { return null; }

			Node? colliderNode = result.ContainsKey("collider") ? result["collider"].AsGodotObject() as Node : null;
			Node3D? wall = FindFadeWallRoot(colliderNode);
			if(IsInstanceValid(wall)) {
				return wall;
			}

			if(result.ContainsKey("rid")) {
				Rid hitRid = (Rid) result["rid"];
				if(hitRid.IsValid) {
					exclusions.Add(hitRid);
					continue;
				}
			}

			return null;
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
	public const float MaxDistance = 8f;
	public const float BufferDistance = 0.25f;

	public float Distance { get; set => field = Math.Clamp(value, MinDistance, MaxDistance); }
	public float Heading { get; set => field = (value + 360) % 360; }
	public float Pitch { get; set => field = Math.Clamp(value, -89, 89); }

	private readonly float RadHDG => Mathf.DegToRad(Heading);
	private readonly float RadPIT => Mathf.DegToRad(Pitch);

	public readonly Vector2 AlignVector(Vector2 direction) => direction.Rotated(-RadHDG);
	public readonly Vector3 AlignVector(Vector3 direction) => direction.Rotated(Vector3.Up, RadHDG);
	public readonly Vector3 CalcDesiredPosition() => Anchor + MathExtensions.ToPolar(RadHDG, RadPIT) * Distance;

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
