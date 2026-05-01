namespace Character;

using Godot;
using ItemSystem;
using Services;

public sealed partial class EnemyRanged : Enemy {
	private static readonly LogService Log = new(nameof(EnemyRanged), enabled: true);

	[Export] private Node3D? StaffCastPoint;
	[Export] private PackedScene? RadiationBoltScene;
	[Export] private StringName StaffAttackAnimation = "";
	[Export] private float RangedAttackCooldown = 1.4f;
	[Export] private float RangedProjectileSpeed = 10.0f;

	protected override void OnAIUpdated(float dt) {
		if(AI.AttackPressed && AttackTarget != null && IsInstanceValid(AttackTarget)) {
			Movement.Face(AttackTarget.GlobalPosition - GlobalPosition, dt);
		}
	}

	protected override void OnAttackTriggered() {
		if(Animator != null && StaffAttackAnimation != "") {
			Animator.SetAttackAnimation(StaffAttackAnimation);
		}
		SpawnRadiationBolt();
	}

	public override void OnAttackFinished() {
		AttackCooldownTimer = RangedAttackCooldown;
		StateMachine.TransitionTo(State.Idle);
	}

	private void SpawnRadiationBolt() {
		if(RadiationBoltScene?.Instantiate() is not RadiationBolt bolt) {
			return;
		}

		if(StaffCastPoint == null || AttackTarget == null || !IsInstanceValid(AttackTarget)) {
			bolt.QueueFree();
			return;
		}

		GetTree().CurrentScene?.AddChild(bolt);
		bolt.GlobalTransform = StaffCastPoint.GlobalTransform;
		bolt.Speed = RangedProjectileSpeed;

		Vector3 targetPosition = AttackTarget.GlobalPosition + new Vector3(0f, 1.0f, 0f);
		Vector3 direction = (targetPosition - StaffCastPoint.GlobalPosition).Normalized();
		bolt.Init(this, direction, Offense.Damage);
		Log.Info($"EnemyRanged fired bolt at {AttackTarget.Name}");
	}
}
