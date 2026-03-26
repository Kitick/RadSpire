namespace Components;

using Godot;
using Root;

public sealed class ChaseAI {
	public Vector3 HorizontalInput { get; private set; }

	public bool SprintHeld { get; private set; }
	public bool CrouchHeld { get; private set; }
	public bool AttackPressed { get; private set; }

	public bool IsMoving => HorizontalInput.Length() >= Numbers.EPSILON;

	private readonly float AttackDistance = 1.5f;

	private readonly Node3D Self;
	private Node3D? Target;

	[Export] private readonly float SprintDistance = 5.0f;
	[Export] private readonly float StopDistance = 1.5f;
	[Export] private readonly float DetectionRadius = 15.0f;

	public ChaseAI(Node3D self) {
		Self = self;
		PickNewWanderAction();
	}

	public void SetTarget(Node3D target) {
		Target = target;
	}

	public Vector3 GetLocation() {
		if(!GodotObject.IsInstanceValid(Target)) { return Vector3.Zero; }
		return Target.GlobalPosition - Self.GlobalPosition;
	}

	private float WanderTimer = 0f;
	private float WanderDuration = 2.0f; // how long to do each action
	private Vector3 CurrentWanderDir = Vector3.Zero;

	private readonly Vector3[] WanderDirections = new[]{
			new Vector3(1, 0, 0),   // right
			new Vector3(-1, 0, 0),  // left
			new Vector3(0, 0, 1),   // forward
			new Vector3(0, 0, -1),  // backward
			Vector3.Zero            // stand still
		};

	public void Update() {
		HorizontalInput = Vector3.Zero;
		SprintHeld = false;
		CrouchHeld = false;
		AttackPressed = false;

		// -------------------------
		// 1. FIRST: see if player is close enough to chase
		// -------------------------
		if(GodotObject.IsInstanceValid(Target)) {
			Vector3 toTarget = Target.GlobalPosition - Self.GlobalPosition;
			toTarget.Y = 0f;
			float dist = toTarget.Length();

			if(dist <= AttackDistance) {
				AttackPressed = true;
				return;
			}

			if(dist <= DetectionRadius && dist > StopDistance) {
				// CHASE PLAYER
				HorizontalInput = toTarget.Normalized();
				SprintHeld = dist > SprintDistance;
				return;
			}
		}

		// -------------------------
		// 2. OTHERWISE: wander
		// -------------------------
		UpdateWander();
	}

	private void UpdateWander() {
		WanderTimer -= 0.025f;

		if(WanderTimer <= 0f) {
			PickNewWanderAction();
		}

		HorizontalInput = CurrentWanderDir;
	}

	private void PickNewWanderAction() {
		// choose a random direction or stop
		int i = (int) (GD.Randi() % WanderDirections.Length);
		CurrentWanderDir = WanderDirections[i];

		// choose how long this action lasts
		WanderDuration = (float) GD.RandRange(1.5f, 4.5f);
		WanderTimer = WanderDuration;
	}
}
