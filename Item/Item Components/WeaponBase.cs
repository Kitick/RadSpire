namespace Components {
	using System;
	using ItemSystem;
	using Character;

	public interface IWeaponBase { WeaponBase Weapon { get; set; } }

	public sealed class WeaponBase : Component<WeaponBaseData>, IItemComponent, IItemEquipable {
		public int priority { get; init; } = 0;
		public int BaseAttack { get; set; } = 10;
		public float AttackSpeed { get; set; } = 1f;

		public string[] getComponentDescription() {
			string[] componentDescriptions = new string[] {
				$"+{BaseAttack} Attack",
				$"{AttackSpeed}x Attack Speed"
			};
			return componentDescriptions;
		}

		public bool Equip<TEntity>(TEntity user) {
			if(user is Player player) {
				player.HoldingSword = true;
				player.Offense.PhysicalDamage += BaseAttack;
			}
			return true;
		}

		public bool Unequip<TEntity>(TEntity user) {
			if(user is Player player) {
				player.HoldingSword = false;
				player.Offense.PhysicalDamage -= BaseAttack;
			}
			return true;
		}

		public WeaponBase(int baseAttack, float attackSpeed) : base(new WeaponBaseData { BaseAttack = baseAttack, AttackSpeed = attackSpeed }) { }
	}

	public static class WeaponExtensions {

	}

	public readonly record struct WeaponBaseData : Services.ISaveData {
		public int BaseAttack { get; init; }
		public float AttackSpeed { get; init; }
	}
}