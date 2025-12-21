using Godot;
using Services;

namespace Components {
	public sealed class Defense : ISaveable<DefenseData> {
		public int DefenseVal { get; set; }
		public float KnockbackResist { get; set; }

		public DefenseData Export() => new DefenseData {
			DefenseVal = DefenseVal,
			KnockbackResist = KnockbackResist,
		};

		public void Import(DefenseData data) {
			DefenseVal = data.DefenseVal;
			KnockbackResist = data.KnockbackResist;
		}
	}

	public readonly struct DefenseData : ISaveData {
		public int DefenseVal { get; init; }
		public float KnockbackResist { get; init; }
	}
}