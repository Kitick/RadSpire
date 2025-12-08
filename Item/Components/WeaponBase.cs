using Godot;
using Services;

namespace Components {
	[GlobalClass]
	public partial class WeaponBase : Resource, IItemComponent, ISaveable<WeaponBaseData> {
		[Export] public int BaseAttack { get; set; } = 10;
		[Export] public float AttackSpeed { get; set; } = 1;
		[Export] public float Range { get; set; } = 1;
		[Export] public float Knockback { get; set; } = 0;
		[Export] public float CriticalChance { get; set; } = 0;
		[Export] public float CriticalMultiplier { get; set; } = 1;
		public WeaponBaseData Serialize() => new WeaponBaseData {
			BaseAttack = BaseAttack,
			AttackSpeed = AttackSpeed,
			Range = Range,
			Knockback = Knockback,
			CriticalChance = CriticalChance,
			CriticalMultiplier = CriticalMultiplier,
		};

		public void Deserialize(in WeaponBaseData data) {
			BaseAttack = data.BaseAttack;
			AttackSpeed = data.AttackSpeed;
			Range = data.Range;
			Knockback = data.Knockback;
			CriticalChance = data.CriticalChance;
			CriticalMultiplier = data.CriticalMultiplier;
		}
	}

	public readonly struct WeaponBaseData : ISaveData {
		public int BaseAttack { get; init; }
		public float AttackSpeed { get; init; }
		public float Range { get; init; }
		public float Knockback { get; init; }
		public float CriticalChance { get; init; }
		public float CriticalMultiplier { get; init; }
	}
}