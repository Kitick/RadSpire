//This file was developed entirely by the RadSpire Development Team.

using System;
using SaveSystem;
using Godot;

namespace Components {
    [GlobalClass]
    public partial class Defense : Resource, IItemComponent, ISaveable<DefenseData> {
        [Export] public float DefenseVal { get; set; } = 0;
        [Export] public float KnockbackResist { get; set; } = 0;

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