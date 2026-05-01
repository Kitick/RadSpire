namespace Character;

using Components;
using Godot;
using Services;

public sealed partial class EnemyMelee : Enemy {
	private static readonly LogService Log = new(nameof(EnemyMelee), enabled: true);

	[Export] private float MeleeAttackCooldown = 1.0f;

	public override void OnAttackFinished() {
		if(AttackTarget != null && IsInstanceValid(AttackTarget) && AttackTarget is IHealth healthTarget) {
			Log.Info($"EnemyMelee attacking {AttackTarget.Name}");
			this.Attack(healthTarget);
			AttackCooldownTimer = MeleeAttackCooldown;
		}
		StateMachine.TransitionTo(State.Idle);
	}
}
