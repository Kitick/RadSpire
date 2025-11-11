using System;
using Godot;
using SaveSystem;

public partial class TopDownCameraRig : Node3D, ISaveable<CameraRigData> {
	[Export] public Node3D? target;
	[Export] private Vector2 defaultCenterZone = new Vector2(1, 1);
	[Export] private float followSpeed = 5.0f;
	[Export] private float mouseSensitivity = 0.01f;
	[Export] private float dragTimePause = 5.0f;
	private float dragSpeed = 15.0f;
	private bool dragging = false;
	private float deltaTime;
	private Timer dragTimer = new Timer();
	private Vector3 targetPosition;
	private float moveThreshold = 0.1f;
	private float outerZoneMultiplier = 10.0f;
	private float maxZoneMultiplier = 20.0f;
	private TopDownCameraPivot? pivot;
	private Vector3 centerOffset = Vector3.Zero;
	private Vector3 dragTargetPosition;
	private Vector3 dragVelocity = Vector3.Zero;
	private float dragVelocityDamp = 8.0f;
	private float dampMaxVelocity = 50.0f;
	private bool skipNextMotion;
	private bool resettingCamera;

	public override void _Ready() {
		GameManager.CameraRig = this;

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
		dragTargetPosition = GlobalPosition;
		skipNextMotion = true;
		resettingCamera = false;
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
		if(resettingCamera) {
			resetCameraPosition();
			if(GlobalPosition.DistanceTo(target.Position) < 0.05f) {
				resettingCamera = false;
			}
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
			if(dragVelocity.LengthSquared() > 0.01f) {
				GlobalPosition += dragVelocity * deltaTime;
			}
		}
	}

	private void followTarget() {
		if(target == null) {
			return;
		}
		Vector3 targetPosition = target.GlobalPosition;
		Vector3 curPosition = GlobalPosition;
		float weight = 1f - Mathf.Exp(-followSpeed * deltaTime);
		GlobalPosition = GlobalPosition.Lerp(targetPosition, weight);
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
			if(skipNextMotion) {
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
			resettingCamera = true;
			skipNextMotion = true;
		}
	}

	private void resetCameraPosition() {
		if(target == null) {
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

	private bool insideNormalDragZone(Vector3 position) {
		if(target == null) {
			return true;
		}
		targetPosition = target.GlobalPosition;
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
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
		Vector2 maxZone = defaultCenterZone * maxZoneMultiplier;
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
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
		Vector2 maxZone = defaultCenterZone * maxZoneMultiplier;
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
		if(target == null) {
			return;
		}
		if(insideNormalDragZone(GlobalPosition)) {
			return;
		}
		Vector3 targetPosition = target.GlobalPosition;
		Vector3 curPosition = GlobalPosition;
		Vector2 outerZone = defaultCenterZone * outerZoneMultiplier;
		Vector3 closestNormalPosition = curPosition;
		closestNormalPosition.X = Mathf.Clamp(curPosition.X, targetPosition.X - outerZone.X / 2, targetPosition.X + outerZone.X / 2);
		closestNormalPosition.Z = Mathf.Clamp(curPosition.Z, targetPosition.Z - outerZone.Y / 2, targetPosition.Z + outerZone.Y / 2);
		float weight = 1f - Mathf.Exp(-followSpeed * deltaTime);
		GlobalPosition = GlobalPosition.Lerp(closestNormalPosition, weight);
	}

	// ISaveable implementation
	public CameraRigData Serialize() {
		return new CameraRigData {
			Position = GlobalPosition,
		};
	}

	public void Deserialize(in CameraRigData data) {
		GlobalPosition = data.Position;
	}
}
