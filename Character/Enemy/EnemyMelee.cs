namespace Character;

using Godot;

public sealed partial class EnemyMelee : Enemy {
	[Export] private float MeleeAttackCooldown = 1.0f;

	public override void OnAttackFinished() {
		AttackCooldownTimer = MeleeAttackCooldown;
		StateMachine.TransitionTo(State.Idle);
	}
}
