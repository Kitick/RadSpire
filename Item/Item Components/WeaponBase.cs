namespace Components {
	using System;
	using ItemSystem;

	public interface IWeaponBase { WeaponBase Weapon { get; set; } }

	public sealed class WeaponBase : Component<WeaponBaseData>, IItemComponent, IItemUseableOnTarget {
		public int BaseAttack { get; set; } = 10;
		public float AttackSpeed { get; set; } = 1f;
		public float Range { get; set; } = 1f;
		public float Knockback { get; set; } = 0f;
		public float CriticalChance { get; set; } = 0f;
		public float CriticalMultiplier { get; set; } = 1f;

		public bool UseOnTarget<TEntity, TTarget>(TEntity user, TTarget target) {
			// Weapon usage on target logic here
			return false;
		}

		public WeaponBase(int baseAttack, float attackSpeed, float range, float knockback, float criticalChance, float criticalMultiplier) : base(new WeaponBaseData { BaseAttack = baseAttack, AttackSpeed = attackSpeed, Range = range, Knockback = knockback, CriticalChance = criticalChance, CriticalMultiplier = criticalMultiplier }) { }
	}

	public static class WeaponExtensions {

	}

	public readonly record struct WeaponBaseData : Services.ISaveData {
		public int BaseAttack { get; init; }
		public float AttackSpeed { get; init; }
		public float Range { get; init; }
		public float Knockback { get; init; }
		public float CriticalChance { get; init; }
		public float CriticalMultiplier { get; init; }
	}
}