namespace Components {
	using System;
	using ItemSystem;

	public interface IWeaponBase { WeaponBase Weapon { get; set; } }

	public sealed class WeaponBase : Component<WeaponBaseData>, IItemComponent, IItemUseableOnTarget {
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

		public bool UseOnTarget<TEntity, TTarget>(TEntity user, TTarget target) {
			// Weapon usage on target logic here
			return false;
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