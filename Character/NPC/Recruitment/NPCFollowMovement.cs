namespace Character.Recruitment;

using Godot;

public sealed class NPCFollowMovement {
	private readonly NPC Npc;
	private readonly float MoveSpeed;
	private readonly float StopDistance;
	private readonly float CatchUpDistance;

	public NPCFollowMovement(NPC npc, float moveSpeed = 2.6f, float stopDistance = 1.75f, float catchUpDistance = 14.0f) {
		Npc = npc;
		MoveSpeed = moveSpeed;
		StopDistance = stopDistance;
		CatchUpDistance = catchUpDistance;
	}

	public void Update(double delta, Node3D target) {
		Vector3 toTarget = target.GlobalPosition - Npc.GlobalPosition;
		toTarget.Y = 0f;
		float distance = toTarget.Length();

		if(distance >= CatchUpDistance) {
			Vector3 offset = -target.GlobalBasis.Z.Normalized() * StopDistance;
			Vector3 teleportPosition = target.GlobalPosition + offset;
			teleportPosition.Y = Npc.GlobalPosition.Y;
			Npc.GlobalPosition = teleportPosition;
			toTarget = target.GlobalPosition - Npc.GlobalPosition;
			toTarget.Y = 0f;
			distance = toTarget.Length();
		}

		if(distance <= StopDistance || distance <= 0.001f) {
			Npc.Velocity = Vector3.Zero;
			Npc.MoveAndSlide();
			return;
		}

		Vector3 direction = toTarget.Normalized();
		Npc.Velocity = direction * MoveSpeed;
		Npc.MoveAndSlide();
		RotateToward(direction, delta);
	}

	private void RotateToward(Vector3 direction, double delta) {
		if(direction.LengthSquared() <= 0.0001f) {
			return;
		}

		float targetRotation = Mathf.Atan2(direction.X, direction.Z);
		Npc.Rotation = new Vector3(
			0f,
			Mathf.LerpAngle(Npc.Rotation.Y, targetRotation, (float)delta * 5f),
			0f
		);
	}
}
