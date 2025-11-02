using Godot;
using System;

public partial class TopDownCameraRig : Node3D {
	[Export] public Node3D? target;
	[Export] private Vector2 defaultCenterZone = new Vector2(6,3);
	[Export] private Vector2 centerZone;
	[Export] private float followSpeed = 5.0f;
	[Export] private float mouseSensitivity = 0.01f;
	[Export] private float dragTimePause = 5.0f;
	private float dragSpeed = 15.0f;
	private bool dragging = false;
	private float deltaTime;
	private Timer dragTimer = new Timer();
	private Vector3 targetPosition;
	private float moveThreshold = 0.1f;
	private float outerZoneMultiplier = 5.0f;
	private float maxZoneMultiplier = 10.0f;
	private TopDownCameraPivot? pivot;
	private Vector3 centerOffset = Vector3.Zero;
	private Vector3 dragTargetPosition;
	private Vector3 dragVelocity = Vector3.Zero;
	private float dragVelocityDamp = 8.0f;
	private float dampMaxVelocity = 50.0f;
	private bool skipNextMotion;

	public override void _Ready() {
		if(target == null) {
			GD.Print("TD Camera Rig needs a target");
		}
		else {
			GD.Print("TD Camera Rig target is set");
			targetPosition = target.GlobalPosition;
			GlobalPosition = targetPosition;
		}
		dragTimer.OneShot = true;
		AddChild(dragTimer);
		dragTimer.Timeout += OnDragTimerTimeout;
		pivot = GetNode<TopDownCameraPivot>("Camera Pivot");
		pivot.ZoomChanged += OnPivotZoomChanged;
		dragTargetPosition = GlobalPosition;
		skipNextMotion = true;
	}

	public override void _PhysicsProcess(double delta) {
		deltaTime = (float)delta;
		if(!dragging && dragTimer.IsStopped()) {
			followTarget();
			skipNextMotion = true;
		}
		if(!dragging && targetMoved()) {
			followTarget();
			skipNextMotion = true;
		}
		if(!dragging && !insideNormalDragZone(GlobalPosition)) {
			moveToNormalZone();
			skipNextMotion = true;
		}
		if(dragging) {
			Vector3 toTarget = dragTargetPosition - GlobalPosition;
			dragVelocity = dragVelocity.Lerp(toTarget * dragSpeed, 1.0f - Mathf.Exp(-dragVelocityDamp * deltaTime));
			dragVelocity = dragVelocity.LimitLength(dampMaxVelocity);
			GlobalPosition += dragVelocity * deltaTime;
		}
		else {
			dragVelocity = dragVelocity.Lerp(Vector3.Zero, 1.0f - Mathf.Exp(-dragVelocityDamp * deltaTime));
			if (dragVelocity.LengthSquared() > 0.01f) {
				GlobalPosition += dragVelocity * deltaTime;
			}
		}
	}

	private void followTarget() {
		if (target == null) {
			return;
		}
		Vector3 targetPosition = target.GlobalPosition + centerOffset;
		Vector3 curPosition = GlobalPosition;
		Vector3 positionDiff = targetPosition - curPosition;
		float xOutPercent = 0.0f;
		float zOutPercent = 0.0f;
		bool inCenterZone = true;
		if(Mathf.Abs(positionDiff.X) > centerZone.X / 2) {
			inCenterZone = false;
			if(positionDiff.X > 0) {
				xOutPercent = Mathf.Abs(positionDiff.X - centerZone.X / 2) / (centerZone.X / 2);
			}
			else {
				xOutPercent = Mathf.Abs(positionDiff.X * -1 - centerZone.X / 2) / (centerZone.X / 2);
			}
		}
		if(Mathf.Abs(positionDiff.Z) > centerZone.Y / 2) {
			inCenterZone = false;
			if(positionDiff.Z > 0) {
				zOutPercent = Mathf.Abs(positionDiff.Z - centerZone.Y / 2) / (centerZone.Y / 2);
			}
			else {
				zOutPercent = Mathf.Abs(positionDiff.Z * -1 - centerZone.Y / 2) / (centerZone.Y / 2);
			}
		}
		if(!inCenterZone) {
			float strength = Mathf.Clamp(Math.Max(xOutPercent, zOutPercent), 0.0f, 1.0f);
			float weight = 1f - Mathf.Exp(-followSpeed * deltaTime * strength);
			GlobalPosition = GlobalPosition.Lerp(targetPosition, weight);
		}
	}

	public override void _Input(InputEvent @event) {
		if(@event is InputEventMouseButton mouseButtonEvent) {
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
		if(dragging && @event is InputEventMouseMotion mouseMotionEvent) {
			if (skipNextMotion) {
				skipNextMotion = false;
				return;
			}
			Vector2 positionChange = mouseMotionEvent.Relative;
			float changeX = -positionChange.X * mouseSensitivity;
			float changeY = 0.0f;
			float changeZ = -positionChange.Y * mouseSensitivity;
			Vector3 totalChange = new Vector3(changeX, changeY, changeZ);
			Vector2 resistance = calculateDragResistance(dragTargetPosition);
			totalChange.X = totalChange.X * (1.0f - resistance.X);
			totalChange.Z = totalChange.Z * (1.0f - resistance.Y);
			dragTargetPosition += totalChange;
		}
		if(@event.IsActionPressed("camera_reset")) {
			dragTimer.Start(0.0000001);
			dragTimer.Stop();
		}
	}

	private void resetCameraPosition() {
		if (target == null) {
			return;
		}
		Vector3 targetPosition = target.GlobalPosition;
		float weight = 1f - Mathf.Exp(-followSpeed * deltaTime);
		GlobalPosition = GlobalPosition.Lerp(targetPosition, weight);
	}

	private void OnDragTimerTimeout() {
		resetCameraPosition();
	}

	private bool targetMoved() {
		if(target is CharacterBody3D body) {
			float targetVelocity = Mathf.Sqrt(body.Velocity.LengthSquared());
			if(targetVelocity > moveThreshold) {
				return true;
			}
		}
		return false;
	}

	private void OnPivotZoomChanged(float zoomFactor, float tiltAngle, float distance) {
		if (pivot == null || target == null){
			return;
		}
		centerZone = defaultCenterZone * zoomFactor;
		float cameraHeight = distance * Mathf.Sin(Mathf.DegToRad(tiltAngle));
		float projectedForward = cameraHeight / Mathf.Tan(Mathf.DegToRad(tiltAngle));
		centerOffset.Z = projectedForward * 0.5f;
		GD.Print($"CenterZone: {centerZone}, CenterOffset: {centerOffset}");
	}

	private bool insideNormalDragZone(Vector3 position) {
		if(target == null) {
			return true;
		}
		targetPosition = target.GlobalPosition;
		Vector2 outerZone = centerZone * outerZoneMultiplier;
		if(position.X < targetPosition.X - outerZone.X / 2 || position.X > targetPosition.X + outerZone.X / 2) {
			return false;
		}
		if(position.Z < targetPosition.Z - outerZone.Y / 2 || position.Z > targetPosition.Z + outerZone.Y / 2) {
			return false;
		}
		return true;
	}

	private bool insideMaxDragZone(Vector3 position) {
		if(target == null) {
			return true;
		}
		Vector2 maxZone = centerZone * maxZoneMultiplier;
		targetPosition = target.GlobalPosition;
		if(position.X < targetPosition.X - maxZone.X / 2 || position.X > targetPosition.X + maxZone.X / 2) {
			return false;
		}
		if(position.Z < targetPosition.Z - maxZone.Y / 2 || position.Z > targetPosition.Z + maxZone.Y / 2) {
			return false;
		}
		return true;
	}

	private Vector2 calculateDragResistance(Vector3 position) {
		if(target == null) {
			return Vector2.Zero;
		}
		Vector2 outerZone = centerZone * outerZoneMultiplier;
		Vector2 maxZone = centerZone * maxZoneMultiplier;
		Vector3 positionDiff = position - target.GlobalPosition;
		float xResist = 0.0f;
		float zResist = 0.0f;
		float xDist = Mathf.Abs(positionDiff.X);
		if(xDist > outerZone.X / 2) {
			float xRange = (maxZone.X / 2) - (outerZone.X / 2);
			float xOver = xDist - (outerZone.X / 2);
			xResist = xOver / xRange;
			xResist = Mathf.Clamp(xResist, 0.0f, 0.9f);
		}
		float zDist = Mathf.Abs(positionDiff.Z);
		if(zDist > outerZone.Y / 2) {
			float zRange = (maxZone.Y / 2) - (outerZone.Y / 2);
			float zOver = zDist - (outerZone.Y / 2);
			zResist = zOver / zRange;
			zResist = Mathf.Clamp(zResist, 0.0f, 0.9f);
		}
		Vector2 resistance = new Vector2(xResist, zResist);
		return resistance;
	}

	private void moveToNormalZone() {
		if (target == null) {
			return;
		}
		if (insideNormalDragZone(GlobalPosition)) {
			return;
		}
		Vector3 targetPosition = target.GlobalPosition;
		Vector3 curPosition = GlobalPosition;
		Vector2 outerZone = centerZone * outerZoneMultiplier;
		Vector3 closestNormalPosition = curPosition;
		closestNormalPosition.X = Mathf.Clamp(curPosition.X, targetPosition.X - outerZone.X / 2, targetPosition.X + outerZone.X / 2);
		closestNormalPosition.Z = Mathf.Clamp(curPosition.Z, targetPosition.Z - outerZone.Y / 2, targetPosition.Z + outerZone.Y / 2);
		float weight = 1f - Mathf.Exp(-followSpeed * deltaTime);
		GlobalPosition = GlobalPosition.Lerp(closestNormalPosition, weight);
	}
}
