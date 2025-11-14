using System;
using Core;
using Godot;
using SaveSystem;

public partial class CameraRig : Node3D, ISaveable<CameraRigData> {
	public Node3D? Target;
	private Vector3 TargetPosition;

	private CameraPivot CameraPivot = null!;

	[Export] private Vector2 defaultCenterZone = new Vector2(1, 1);
	[Export] private float FollowSpeed = 5.0f;
	[Export] private float mouseSensitivity = 0.01f;
	[Export] private float dragTimePause = 5.0f;
	private float dragSpeed = 15.0f;
	private bool dragging = false;
	private Timer dragTimer = new Timer();
	private float moveThreshold = 0.1f;
	private float outerZoneMultiplier = 10.0f;
	private float maxZoneMultiplier = 20.0f;
	private Vector3 dragTargetPosition;
	private Vector3 dragVelocity = Vector3.Zero;
	private float dragVelocityDamp = 8.0f;
	private float dampMaxVelocity = 50.0f;
	private bool skipNextMotion;
	private bool resettingCamera;

	private const string CAMERA_PIVOT = "Camera Pivot";

	public override void _Ready() {
		if(Target != null) {
			InitializeTarget();
		}

		dragTimer.OneShot = true;
		AddChild(dragTimer);
		dragTimer.Timeout += OnDragTimerTimeout;
		CameraPivot = GetNode<CameraPivot>(CAMERA_PIVOT);
		dragTargetPosition = GlobalPosition;
		skipNextMotion = true;
		resettingCamera = false;
	}

	private void InitializeTarget() {
		GD.Print("TD Camera Rig target is set");
		TargetPosition = Target!.GlobalPosition;
		GlobalPosition = TargetPosition;
	}

	public void SetTarget(Node3D target) {
		Target = target;
		InitializeTarget();
	}

	public override void _PhysicsProcess(double delta) {
		if(Target == null) { return; }

		float dt = (float)delta;

		if(!dragging && dragTimer.IsStopped()) {
			FollowTarget(dt);
			skipNextMotion = true;
		}
		if(!dragging && TargetHasMoved()) {
			FollowTarget(dt);
			skipNextMotion = true;
		}
		if(!dragging && !IsInsideNormalDragZone(GlobalPosition)) {
			MoveToNormalZone(dt);
			skipNextMotion = true;
		}
		if(resettingCamera) {
			FollowTarget(dt);
			if(GlobalPosition.DistanceTo(Target.Position) < 0.05f) {
				resettingCamera = false;
			}
			skipNextMotion = true;
		}
		if(dragging) {
			Vector3 toTarget = dragTargetPosition - GlobalPosition;
			dragVelocity = dragVelocity.SmoothLerp(toTarget * dragSpeed, dragVelocityDamp, dt);
			dragVelocity = dragVelocity.LimitLength(dampMaxVelocity);
			GlobalPosition += dragVelocity * dt;
		}
		else {
			dragVelocity = dragVelocity.SmoothLerp(Vector3.Zero, dragVelocityDamp, dt);
			if(dragVelocity.LengthSquared() > 0.01f) {
				GlobalPosition += dragVelocity * dt;
			}
		}
	}

	private void FollowTarget(float dt) {
		GlobalPosition = GlobalPosition.SmoothLerp(TargetPosition, FollowSpeed, dt);
	}

	public override void _Input(InputEvent input) {
		if(input is InputEventMouseButton mouseButtonEvent) {
			if(mouseButtonEvent.ButtonIndex == MouseButton.Right) {
				if(!dragging && mouseButtonEvent.Pressed) {
					dragging = true;
					dragVelocity = Vector3.Zero;
					dragTargetPosition = GlobalPosition;
					skipNextMotion = true;
				}
				else if(dragging && !mouseButtonEvent.Pressed) {
					dragging = false;
					dragTimer.Start(dragTimePause);
				}
			}
		}
		if(dragging && input is InputEventMouseMotion mouseMotionEvent) {
			if(skipNextMotion) {
				skipNextMotion = false;
				return;
			}
			Vector2 positionChange = mouseMotionEvent.Relative;
			float changeX = -positionChange.X * mouseSensitivity;
			float changeY = 0.0f;
			float changeZ = -positionChange.Y * mouseSensitivity;
			Vector3 totalChange = new Vector3(changeX, changeY, changeZ);
			Vector2 resistance = CalculateDragResistance(dragTargetPosition);
			totalChange.X = totalChange.X * (1.0f - resistance.X);
			totalChange.Z = totalChange.Z * (1.0f - resistance.Y);
			dragTargetPosition += totalChange;
		}
		if(input.IsActionPressed(Actions.CameraReset)) {
			resettingCamera = true;
			skipNextMotion = true;
		}
	}

	private void OnDragTimerTimeout() {
		FollowTarget(0.0f);
	}

	private bool TargetHasMoved() {
		if(Target is CharacterBody3D body) {
			float targetVelocity = Mathf.Sqrt(body.Velocity.LengthSquared());
			if(targetVelocity > moveThreshold) {
				return true;
			}
		}
		return false;
	}

	private bool IsInsideNormalDragZone(Vector3 position) {
		TargetPosition = Target.GlobalPosition;
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
		Vector3 horizontalDiff = (position - TargetPosition).Horizontal();
		return Mathf.Abs(horizontalDiff.X) <= outerZone.X / 2 && Mathf.Abs(horizontalDiff.Z) <= outerZone.Y / 2;
	}

	private Vector2 CalculateDragResistance(Vector3 position) {
		Vector3 positionDiff = (position - Target.GlobalPosition).Horizontal();
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
		Vector2 maxZone = defaultCenterZone * maxZoneMultiplier;

		float xResist = CalculateAxisResistance(Mathf.Abs(positionDiff.X), outerZone.X, maxZone.X);
		float zResist = CalculateAxisResistance(Mathf.Abs(positionDiff.Z), outerZone.Y, maxZone.Y);

		return new Vector2(xResist, zResist);
	}

	private float CalculateAxisResistance(float distance, float outerBound, float maxBound) {
		if(distance <= outerBound / 2) { return 0.0f; }

		float range = (maxBound / 2) - (outerBound / 2);
		float over = distance - (outerBound / 2);
		float resist = over / range;
		return Mathf.Clamp(resist, 0.0f, 0.9f);
	}

	private void MoveToNormalZone(float dt) {
		if(IsInsideNormalDragZone(GlobalPosition)) { return; }

		Vector3 targetPosition = Target.GlobalPosition;
		Vector3 curPosition = GlobalPosition;
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
		Vector3 closestNormalPosition = curPosition.Horizontal() + targetPosition.Vertical();
		closestNormalPosition.X = Mathf.Clamp(closestNormalPosition.X, targetPosition.X - outerZone.X / 2, targetPosition.X + outerZone.X / 2);
		closestNormalPosition.Z = Mathf.Clamp(closestNormalPosition.Z, targetPosition.Z - outerZone.Y / 2, targetPosition.Z + outerZone.Y / 2);
		GlobalPosition = GlobalPosition.SmoothLerp(closestNormalPosition, FollowSpeed, dt);
	}

	public CameraRigData Serialize() => new CameraRigData {
		Position = GlobalPosition,
		CameraPivotData = CameraPivot.Serialize(),
	};

	public void Deserialize(in CameraRigData data) {
		GlobalPosition = data.Position;
		CameraPivot.Deserialize(data.CameraPivotData);
	}
}

namespace SaveSystem {
	public readonly record struct CameraRigData : ISaveData {
		public Vector3 Position { get; init; }
		public Vector3 CenterOffset { get; init; }
		public CameraPivotData CameraPivotData { get; init; }
	}
}