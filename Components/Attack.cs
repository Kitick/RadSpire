using System;
using Services;

namespace Components {
	public interface IAttack { Attack Attack { get; } }

	public sealed class Attack : ISaveable<AttackData> {
		public int Damage;

		public Attack(int damage) {
			Damage = damage;
		}

		public AttackData Export() => new AttackData {
			Damage = Damage,
		};

		public void Import(AttackData data) {
			Damage = data.Damage;
		}
	}

	public static class AttackExtensions {
		public static void Attack<TAttacker, TDefender>(this TAttacker attacker, TDefender defender)
		where TAttacker : IAttack
		where TDefender : IHealth {
			defender.Hurt(attacker.Attack.Damage);
		}

		public static void AttackDurable<TWeapon, TDefender>(this TWeapon weapon, TDefender defender)
		where TWeapon : IAttack, IDurability
		where TDefender : IHealth {
			weapon.Attack(defender);
			weapon.Damage(1);
		}
	}

	public readonly record struct AttackData : ISaveData {
		public int Damage { get; init; }
	}
}