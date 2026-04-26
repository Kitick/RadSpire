namespace Components;

using System;
using ItemSystem;

public static class Interactions {
	public static void Attack<TAttacker, TDefender>(this TAttacker attacker, TDefender defender)
	where TAttacker : IOffense
	where TDefender : IHealth {
		int damage = attacker.Offense.Damage;

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
