using System;
using SaveSystem;

namespace Components {
    public enum WeaponType { Melee, Range, Magic }

    public class WeaponBaseData : ISaveable<WeaponBaseDataData> {

        public WeaponType Type {
            get;
            set;
        }

        public int CurrentDurability {
            get;
            set {
                value = Math.Clamp(value, 0, MaxDurability);
                if(field == value) { return; }
                field = value;
                if(field == 0) {
                    DurabilityZeroed?.Invoke();
                }
            }
        }

        public event Action? DurabilityZeroed;

        public int MaxDurability {
            get;
            set {
                field = Math.Max(1, value);
                CurrentDurability = Math.Min(CurrentDurability, field);
            }
        }

        public WeaponBaseData(int maxDurability) {
            MaxDurability = maxDurability;
            CurrentDurability = maxDurability;
        }

        public WeaponBaseDataData Serialize() => new WeaponBaseDataData {
            Type = Type,
            CurrentDurability = CurrentDurability,
            MaxDurability = MaxDurability
        };

        public void Deserialize(in WeaponBaseDataData data) {
            Type = data.Type;
            CurrentDurability = data.CurrentDurability;
            MaxDurability = data.MaxDurability;
        }
    }
    
	public static class WeaponBaseDataExtensions {
		public static bool IsWorking(this WeaponBaseData weaponBaseData) => weaponBaseData.CurrentDurability > 0;
		public static bool IsNew(this WeaponBaseData weaponBaseData) => weaponBaseData.CurrentDurability == weaponBaseData.MaxDurability;
		public static bool IsDamaged(this WeaponBaseData weaponBaseData) => weaponBaseData.CurrentDurability < weaponBaseData.MaxDurability;
		public static bool IsBroken(this WeaponBaseData weaponBaseData) => weaponBaseData.CurrentDurability == 0;

		public static float DurabilityPercent(this WeaponBaseData weaponBaseData) =>
			(float)weaponBaseData.CurrentDurability / weaponBaseData.MaxDurability;
	}
}

namespace SaveSystem {
    public readonly struct WeaponBaseDataData : ISaveData {
        public Components.WeaponType Type { get; init; }
        public int CurrentDurability { get; init; }
        public int MaxDurability { get; init; }
    }
}