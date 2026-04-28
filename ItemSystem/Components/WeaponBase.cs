namespace ItemSystem;

using Character;
using Components;

public interface IWeaponBase { WeaponBase Weapon { get; set; } }

public sealed class WeaponBase : Component<WeaponBaseData>, IItemComponent, IItemEquipable {
	public enum WeaponVisualType { Sword, Staff }

	public int priority { get; init; } = 0;
	public int BaseAttack { get; set; } = 10;
	public float AttackSpeed { get; set; } = 1f;
	public WeaponVisualType VisualType { get; set; } = WeaponVisualType.Sword;

	public string[] getComponentDescription() {
		string[] componentDescriptions = [
			$"+{BaseAttack} Attack",
			$"{AttackSpeed}x Attack Speed"
		];
		return componentDescriptions;
	}

	public bool Equip<TEntity>(TEntity user) {
		if(user is Player player) {
			player.Offense.Damage += BaseAttack;
			switch(VisualType) {
				case WeaponVisualType.Staff:
					player.HoldingStaff = true;
					if(player.StaffMesh != null) {
						player.StaffMesh.Visible = true;
					}
					break;
				default:
					player.HoldingSword = true;
					if(player.SwordMesh != null) {
						player.SwordMesh.Visible = true;
					}
					break;
			}
		}
		return true;
	}

	public bool Unequip<TEntity>(TEntity user) {
		if(user is Player player) {
			player.Offense.Damage -= BaseAttack;
			switch(VisualType) {
				case WeaponVisualType.Staff:
					player.HoldingStaff = false;
					if(player.StaffMesh != null) {
						player.StaffMesh.Visible = false;
					}
					break;
				default:
					player.HoldingSword = false;
					if(player.SwordMesh != null) {
						player.SwordMesh.Visible = false;
					}
					break;
			}
		}
		return true;
	}

	public WeaponBase(int baseAttack, float attackSpeed, WeaponVisualType visualType = WeaponVisualType.Sword)
		: base(new WeaponBaseData { BaseAttack = baseAttack, AttackSpeed = attackSpeed, VisualType = visualType }) {
		BaseAttack = baseAttack;
		AttackSpeed = attackSpeed;
		VisualType = visualType;
	}
}

public readonly record struct WeaponBaseData : Services.ISaveData {
	public int BaseAttack { get; init; }
	public float AttackSpeed { get; init; }
	public WeaponBase.WeaponVisualType VisualType { get; init; }
}
