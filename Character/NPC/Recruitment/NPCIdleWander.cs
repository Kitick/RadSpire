namespace Character.Recruitment;

using Godot;

public sealed class NPCIdleWander {
	private readonly NPC Npc;
	private readonly RandomNumberGenerator Rng = new();
	private readonly float MoveSpeed;
	private readonly float WanderRadius;
	private readonly float ArrivalDistance;

	private Vector3 HomePosition;
	private Vector3 HomeRotation;
	private Vector3 CurrentTarget;
	private float PauseTimer;

	public NPCIdleWander(NPC npc, float moveSpeed = 1.5f, float wanderRadius = 2.5f, float arrivalDistance = 0.35f) {
		Npc = npc;
		MoveSpeed = moveSpeed;
		WanderRadius = wanderRadius;
		ArrivalDistance = arrivalDistance;
		HomePosition = npc.GlobalPosition;
		HomeRotation = npc.GlobalRotation;
		CurrentTarget = HomePosition;
		PauseTimer = 0f;
	}

	public void SetHome(Vector3 homePosition, Vector3 homeRotation) {
		HomePosition = homePosition;
		HomeRotation = homeRotation;
		CurrentTarget = homePosition;
		PauseTimer = 0f;
	}

	public void Update(double delta) {
		PauseTimer -= (float)delta;
		Vector3 toTarget = CurrentTarget - Npc.GlobalPosition;
		toTarget.Y = 0f;

		if(toTarget.Length() <= ArrivalDistance) {
			Npc.Velocity = Vector3.Zero;
			Npc.MoveAndSlide();

			if(PauseTimer <= 0f) {
				PickNextTarget();
			}

			RotateTowardHome(delta);
			return;
		}

		Vector3 direction = toTarget.Normalized();
		Npc.Velocity = direction * MoveSpeed;
		Npc.MoveAndSlide();
		RotateToward(direction, delta);
	}

	private void PickNextTarget() {
		PauseTimer = Rng.RandfRange(1.5f, 4.0f);
		Vector2 offset = Vector2.FromAngle(Rng.RandfRange(0f, Mathf.Tau)) * Rng.RandfRange(0.5f, WanderRadius);
		CurrentTarget = HomePosition + new Vector3(offset.X, 0f, offset.Y);
	}

	private void RotateTowardHome(double delta) {
		Npc.Rotation = new Vector3(
			0f,
			Mathf.LerpAngle(Npc.Rotation.Y, HomeRotation.Y, (float)delta * 2f),
			0f
		);
	}

	private void RotateToward(Vector3 direction, double delta) {
		if(direction.LengthSquared() <= 0.0001f) {
			return;
		}

		float targetRotation = Mathf.Atan2(direction.X, direction.Z);
		Npc.Rotation = new Vector3(
			0f,
			Mathf.LerpAngle(Npc.Rotation.Y, targetRotation, (float)delta * 4f),
			0f
		);
	}
}
