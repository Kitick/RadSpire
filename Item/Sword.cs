using Godot;
using Components;
using SaveSystem;

public partial class Sword : Node3D {

	private WeaponHitbox HitBox = null!;
	
	public WeaponBase Stats { get; } = new WeaponBase {
		BaseAttack = 20,
		AttackSpeed = 1.5f,
		Range = 1.75f,
		CriticalChance = 0.1f,
		CriticalMultiplier = 2.0f,
		Knockback = 0f,
	};

	public override void _Ready() {
		
	}
	public WeaponBaseData Serialize() => Stats.Serialize();
	public void Deserialize(WeaponBaseData data) => Stats.Deserialize(data);
}