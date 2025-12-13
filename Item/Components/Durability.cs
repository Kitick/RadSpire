//This file was developed entirely by the RadSpire Development Team.

using System;
using SaveSystem;
using Godot;

namespace Components {
    [GlobalClass]
    public partial class Durability : Resource, IItemComponent, ISaveable<DurabilityData> {
        [Export] public int CurrentDurability {
            get;
            set {
                DurabilityChanged?.Invoke(CurrentDurability, value);
                value = Math.Clamp(value, 0, MaxDurability);
                if(field == value) { return; }
                field = value;
                if(field == 0) {
                    DurabilityZeroed?.Invoke();
                }
            }
        }

        public event Action? DurabilityZeroed;
        public event Action<int, int>? DurabilityChanged;

        [Export] public int MaxDurability {
            get;
            set {
                DurabilityChanged?.Invoke(CurrentDurability, value);
                field = Math.Max(1, value);
                CurrentDurability = Math.Min(CurrentDurability, field);
            }
        }

        public Durability(int maxDurability) {
            MaxDurability = maxDurability;
            CurrentDurability = maxDurability;
        }

        public DurabilityData Serialize() => new DurabilityData {
            currentDurability = CurrentDurability,
            maxDurability = MaxDurability,
        };

        public void Deserialize(in DurabilityData data) {
            CurrentDurability = data.currentDurability;
            MaxDurability = data.maxDurability;
        }
    }
    
	public static class DurabilityExtensions {
		public static bool IsNew(this Durability durability) => durability.CurrentDurability == durability.MaxDurability;
		public static bool IsDamaged(this Durability durability) => durability.CurrentDurability < durability.MaxDurability;
		public static bool IsBroken(this Durability durability) => durability.CurrentDurability == 0;

        public static float DurabilityPercent(this Durability durability) =>
            (float) durability.CurrentDurability / durability.MaxDurability;

        public static void Repair(this Durability durability, int amount) {
            durability.CurrentDurability = durability.CurrentDurability + amount;
        }
        
        public static void Damage(this Durability durability, int amount) {
            durability.CurrentDurability = durability.CurrentDurability - amount;
        }
	}
}

namespace SaveSystem {
    public readonly struct DurabilityData : ISaveData {
        public int currentDurability { get; init; }
        public int maxDurability { get; init; }
    }
}