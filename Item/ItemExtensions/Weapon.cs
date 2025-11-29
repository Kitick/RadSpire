using System;
using Components;
using Core;
using Godot;
using SaveSystem;

public class Weapon : Item, ISaveable<ItemData> {
    public float BaseAttack { get; set; }
    public float AttackSpeed { get; set; }
    public float Range { get; set; }
    public float CriticalChance { get; set; }
    public float CriticalMultiplier { get; set; }

	public WeaponData Serialize() => new WeaponData {
        ItemData = base.Serialize(),
        BaseAttack = BaseAttack,
        AttackSpeed = AttackSpeed,
        Range = Range,
        CriticalChance = CriticalChance,
        CriticalMultiplier = CriticalMultiplier,
    };

	public void Deserialize(in WeaponData data) {
        base.Deserialize(data.ItemData);
        BaseAttack = data.BaseAttack;
        AttackSpeed = data.AttackSpeed;
        Range = data.Range;
        CriticalChance = data.CriticalChance;
        CriticalMultiplier = data.CriticalMultiplier;
	}
}

namespace SaveSystem {
    public readonly record struct WeaponData : ISaveData {
        public ItemData ItemData { get; init; }
        public float BaseAttack { get; init; }
        public float AttackSpeed { get; init; }
        public float Range { get; init; }
        public float CriticalChance { get; init; }
        public float CriticalMultiplier { get; init; }
        public DurabilityData? Durability { get; init; }
        public CraftingData? Crafting { get; init; }
    }
}