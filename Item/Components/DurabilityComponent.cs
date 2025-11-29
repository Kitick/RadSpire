using System;
using SaveSystem;

namespace Components {
    public class Durability : ISaveable<DurabilityData> {
        public int CurrentDurability {
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

        public int MaxDurability {
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
            CurrentDurability = CurrentDurability,
            MaxDurability = MaxDurability,
        };

        public void Deserialize(in DurabilityData data) {
            CurrentDurability = data.CurrentDurability;
            MaxDurability = data.MaxDurability;
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
        public int CurrentDurability { get; init; }
        public int MaxDurability { get; init; }
    }
}