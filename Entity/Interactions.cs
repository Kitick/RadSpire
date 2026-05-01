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
		int damage = attacker.Offense.RollDamage();
		if(attacker is IAttackModifier modifier) {
			damage = (int) Math.Round(damage * modifier.GetAttackMultiplier());
		}

		if(defender is IDefense defendable) {
			damage = Math.Max(0, damage - defendable.Defense.Armor);
		}

		defender.Hurt(damage);

		if(attacker is IDurable weapon) {
			weapon.Damage(1);
		}
	}

	public static int RollDamage(this Offense offense, int? baseDamage = null) {
		float roll = offense.DamageVariance > 0f
			? 1f + (float)(Rng.NextDouble() * 2.0 - 1.0) * offense.DamageVariance
			: 1f;
		return Math.Max(1, (int) Math.Round((baseDamage ?? offense.Damage) * roll));
	}

	public static void HealWith<TEntity, TItem>(this TEntity target, TItem item)
	where TEntity : IHealth
	where TItem : IHealItem => target.Heal(item.Heal.HealAmount);
}
