using System;
using SaveSystem;

namespace Components {
    public enum WeaponType { Melee, Range, Magic }

	public class WeaponBaseData : ISaveable<WeaponBaseDataData> {

        public WeaponType Type {
            get;
            set;
        }





		public WeaponBaseDataData Serialize() => new WeaponBaseDataData {
            Type = Type
        };

		public void Deserialize(in WeaponBaseDataData data) {
            Type = data.Type;
		}
	}
}

namespace SaveSystem {
	public readonly struct WeaponBaseDataData : ISaveData {
        public Components.WeaponType Type { get; init; }
	}
}