namespace Components;

using System;
using ItemSystem;

public interface IAttackModifier {
	float GetAttackMultiplier();
}

public static class Interactions {
	private static readonly Random Rng = new();

	public static void Attack<TAttacker, TDefender>(this TAttacker attacker, TDefender defender)
	where TAttacker : IOffense
	where TDefender : IHealth {
		int damage = attacker.Offense.Damage;
		if(attacker is IAttackModifier modifier) {
			damage = (int) Math.Round(damage * modifier.GetAttackMultiplier());
		}

		float variance = attacker.Offense.DamageVariance;
		if(variance > 0f) {
			float roll = 1f + (float)(Rng.NextDouble() * 2.0 - 1.0) * variance;
			damage = Math.Max(1, (int)Math.Round(damage * roll));
		}

		if(defender is IDefense defendable) {
			damage = Math.Max(0, damage - defendable.Defense.Armor);
		}

		defender.Hurt(damage);

		if(attacker is IDurable weapon) {
			weapon.Damage(1);
		}
	}

	public static void HealWith<TEntity, TItem>(this TEntity target, TItem item)
	where TEntity : IHealth
	where TItem : IHealItem => target.Heal(item.Heal.HealAmount);
}
