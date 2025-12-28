using Godot;
using Services;

namespace Components {
	public interface IDefense { Defense Defense { get; } }

	public sealed class Defense : ISaveable<DefenseData> {
		public int PhysicalDefense;
		public int MagicDefense;

		public Defense(int phys, int mag) {
			PhysicalDefense = phys;
			MagicDefense = mag;
		}

		public DefenseData Export() => new DefenseData {
			PhysicalDefense = PhysicalDefense,
			MagicDefense = MagicDefense,
		};

		public void Import(DefenseData data) {
			PhysicalDefense = data.PhysicalDefense;
			MagicDefense = data.MagicDefense;
		}
	}

	public readonly record struct DefenseData : ISaveData {
		public int PhysicalDefense { get; init; }
		public int MagicDefense { get; init; }
	}
}