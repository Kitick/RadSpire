using System;
using SaveSystem;

namespace Components {
    public class WeaponBase : IItemComponent, ISaveable<WeaponBaseData> {
        public float BaseAttack { get; set; } = 10;
        public float AttackSpeed { get; set; } = 1;
        public float Range { get; set; } = 1;
        public float Knockback { get; set; } = 0;
        public float CriticalChance { get; set; } = 0;
        public float CriticalMultiplier { get; set; } = 1;
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
}

namespace SaveSystem {
    public readonly struct WeaponBaseData : ISaveData {
        public float BaseAttack { get; init; }
        public float AttackSpeed { get; init; }
        public float Range { get; init; }
        public float Knockback { get; init; }
        public float CriticalChance { get; init; }
        public float CriticalMultiplier { get; init; }
    }
}