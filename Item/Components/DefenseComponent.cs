using System;
using SaveSystem;

namespace Components {
    public class Defense : IItemComponent, ISaveable<DefenseData> {
        public float DefenseVal { get; set; }
        public float KnockbackResist { get; set; }

        public DefenseData Serialize() => new DefenseData {
            DefenseVal = DefenseVal,
            KnockbackResist = KnockbackResist,
        };

        public void Deserialize(in DefenseData data) {
            DefenseVal = data.DefenseVal;
            KnockbackResist = data.KnockbackResist;
        }
    }
}

namespace SaveSystem {
    public readonly struct DefenseData : ISaveData {
        public float DefenseVal { get; init; }
        public float KnockbackResist { get; init; }
    }
}