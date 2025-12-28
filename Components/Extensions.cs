using System;

namespace Components {
	public interface IOnChanged<T> {
		event Action<T, T> OnChanged;
	}

	public static class Extensions {
		public static Action When<TComp, TData>(this TComp target, Action<TData, TData> callback) where TComp : IOnChanged<TData> {
			target.OnChanged += callback;
			return () => target.OnChanged -= callback;
		}
	}

	public static class Interactions {
		public static void Attack<TAttacker, TDefender>(this TAttacker attacker, TDefender defender)
		where TAttacker : IOffense
		where TDefender : IHealth {
			int physicalDamage = attacker.Offense.PhysicalDamage;
			int magicDamage = attacker.Offense.MagicDamage;

			int physicalDefense = 0;
			int magicDefense = 0;

			if(defender is IDefense defense) {
				physicalDefense = defense.Defense.PhysicalDefense;
				magicDefense = defense.Defense.MagicDefense;
			}

			physicalDamage = Math.Max(0, physicalDamage - physicalDefense);
			magicDamage = Math.Max(0, magicDamage - magicDefense);

			defender.Hurt(physicalDamage + magicDamage);

			if(attacker is IDurable weapon) {
				weapon.Damage(1);
			}
		}

		public static void HealWith<TEntity, TItem>(this TEntity target, TItem item)
		where TEntity : IHealth
		where TItem : IHealItem {
			target.Heal(item.Item.HealAmount);
		}
	}
}