namespace ItemSystem;

using Character;
using Components;
using Godot;
using Services;

public partial class Sword : Area3D {
	private static readonly LogService Log = new(nameof(Sword), enabled: true);

	private Node3D WeaponOwner = null!;

	public override void _Ready() {
		WeaponOwner = GetOwner<Node3D>();
		Monitoring = false;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body) {
		if(body == WeaponOwner) {
			return;
		}

		if(WeaponOwner is Enemy enemy && body is Player player && enemy.SceneFilePath.Contains("BossEnemy.tscn")) {
			player.TriggerBossHitReaction(WeaponOwner.GlobalPosition);
		}

		if(WeaponOwner is IOffense attacker && body is IHealth health) {
			attacker.Attack(health);
			return;
		}

		if(body is IHealth fallbackHealth) {
			fallbackHealth.Hurt(10);
		}
	}
}
