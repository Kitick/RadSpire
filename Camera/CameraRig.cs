using System;
using Core;
using Godot;
using SaveSystem;

public partial class CameraRig : Node3D, ISaveable<CameraRigData> {
	public enum CameraState { Idle, Following, Dragging, Cooldown };

	public CameraState CurrentState { get; private set; } = CameraState.Idle;

	private CameraPivot CameraPivot = null!;

	public Node3D? Target;

	public float FollowSpeed = 5.0f;

	public MouseButton DragMouseButton = MouseButton.Right;
	public float MouseSensitivity = 0.01f;

	private Timer DragTimer = new Timer();

	private Vector3 DragTarget = Vector3.Zero;
	private Vector3 DragVelocity = Vector3.Zero;

	public float DragSpeed = 12.0f;
	public float DragDamp = 8.0f;

	private readonly TimeSpan Cooldown = TimeSpan.FromSeconds(3);

	private const string CAMERA_PIVOT = "Camera Pivot";

	public override void _Ready() {
		AddChild(DragTimer);
		DragTimer.OneShot = true;
		DragTimer.Timeout += OnDragTimerTimeout;

		CameraPivot = GetNode<CameraPivot>(CAMERA_PIVOT);
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float) delta;

		UpdateState();

		if(CurrentState == CameraState.Following) {
			FollowTarget(Target!, dt);
		}

		Vector3 TargetVelocity;

		if(CurrentState == CameraState.Dragging) {
			TargetVelocity = (DragTarget - GlobalPosition) * DragSpeed;
		}
		else {
			TargetVelocity = Vector3.Zero;
		}

		DragVelocity = DragVelocity.SmoothLerp(TargetVelocity, DragDamp, dt);

		GlobalPosition += DragVelocity * dt;
	}

	public override void _Input(InputEvent input) {
		if(input.IsActionPressed(Actions.CameraReset)) {
			CurrentState = CameraState.Following;
			return;
		}

		if(input is InputEventMouseButton mouseEvent) {
			HandleMouseEvent(mouseEvent);
		}
		else if(input is InputEventMouseMotion mouseMotion && CurrentState == CameraState.Dragging) {
			HandleMouseMotion(mouseMotion);
		}
	}

	private void UpdateState() {
		if(Target == null) {
			CurrentState = CameraState.Idle;
		}
		else if(CurrentState != CameraState.Dragging && TargetHasMoved()) {
			CurrentState = CameraState.Following;
		}
	}

	private void FollowTarget(Node3D Target, float dt) {
		GlobalPosition = GlobalPosition.SmoothLerp(Target.GlobalPosition, FollowSpeed, dt);
	}

	private void HandleMouseEvent(InputEventMouseButton input) {
		if(input.ButtonIndex != DragMouseButton) { return; }

		if(CurrentState != CameraState.Dragging && input.Pressed) {
			CurrentState = CameraState.Dragging;
			DragTimer.Stop();

			DragTarget = GlobalPosition;
		}
		else if(CurrentState == CameraState.Dragging && !input.Pressed) {
			CurrentState = CameraState.Cooldown;
			DragTimer.Start(Cooldown.Seconds);
		}
	}

	private void HandleMouseMotion(InputEventMouseMotion motion) {
		Vector2 mouseDelta = -motion.Relative * MouseSensitivity;

		DragTarget += new Vector3(mouseDelta.X, 0, mouseDelta.Y);
	}

	private void OnDragTimerTimeout() {
		CurrentState = CameraState.Following;
	}

	private bool TargetHasMoved() {
		if(Target is not CharacterBody3D body) { return false; }

		float targetSpeed = body.Velocity.Length();

		return targetSpeed > Numbers.EPSILON;
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