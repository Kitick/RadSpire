using System;
using SaveSystem;

namespace Components {
    public class WeaponBase : IItemComponent, ISaveable<WeaponBaseData> {
        public float BaseAttack { get; set; }
        public float AttackSpeed { get; set; }
        public float Range { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalMultiplier { get; set; }
        public WeaponBaseData Serialize() => new WeaponBaseData {
            BaseAttack = BaseAttack,
            AttackSpeed = AttackSpeed,
            Range = Range,
            CriticalChance = CriticalChance,
            CriticalMultiplier = CriticalMultiplier,
        };

        public void Deserialize(in WeaponBaseData data) {
            BaseAttack = data.BaseAttack;
            AttackSpeed = data.AttackSpeed;
            Range = data.Range;
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
        public float CriticalChance { get; init; }
        public float CriticalMultiplier { get; init; }
    }
}