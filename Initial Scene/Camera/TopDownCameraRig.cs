using Godot;
using System;

public partial class TopDownCameraRig : Node3D {
	[Export] public Node3D target;
	[Export] private Vector2 centerZone = new Vector2(5, 4);
	[Export] private float followSpeed = 5.0f;
	[Export] private float dragSpeed = 2.0f;
	[Export] private float dragTimePause = 5.0f;
	private bool dragging = false;
	private float deltaTime;
	private Timer dragTimer = new Timer();

	private Vector3 targetPosition;

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
	}

	public override void _PhysicsProcess(double delta) {
		deltaTime = (float)delta;
		if(!dragging && dragTimer.IsStopped()) {
			followTarget();
		}
	}

	private void followTarget() {
		Vector3 targetPosition = target.GlobalPosition;
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
				}
				else if(dragging && !mouseButtonEvent.Pressed) {
					dragging = false;
					dragTimer.Start(dragTimePause);
				}
			}
		}
		if(dragging && @event is InputEventMouseMotion mouseMotionEvent) {
			Vector2 positionChange = mouseMotionEvent.Relative;
			float finalX = GlobalPosition.X - positionChange.X;
			float finalY = GlobalPosition.Y;
			float finalZ = GlobalPosition.Z - positionChange.Y;
			Vector3 finalPosition = new Vector3(finalX, finalY, finalZ);
			float weight = 1f - Mathf.Exp(-dragSpeed * deltaTime);
			GlobalPosition = GlobalPosition.Lerp(finalPosition, weight);
		}
		if(@event.IsActionPressed("camera_reset")) {
			dragTimer.Start(0.0000001);
			dragTimer.Stop();
		}
	}

	private void resetCameraPosition() {
		float weight = 1f - Mathf.Exp(-followSpeed * deltaTime);
		GlobalPosition = GlobalPosition.Lerp(target.GlobalPosition, weight);
	}

	private void OnDragTimerTimeout() {
		resetCameraPosition();
	}
}
